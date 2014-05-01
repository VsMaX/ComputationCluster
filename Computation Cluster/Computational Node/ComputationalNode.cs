using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;
using System.Threading;
using DynamicVehicleRoutingProblem;

namespace Computational_Node
{
    public class ComputationnalNode : BaseNode
    {
        private Thread sendingStatusThread;
        private CancellationTokenSource sendingStatusThreadCancellationTokenSource;

        private Thread processingQueueThread;
        private CancellationTokenSource processingThreadCancellationTokenSource;

        private Queue<PartialProblemsMessage> partialProblemsQueue;
        private Queue<SolutionsMessage> partialSolutionsQueue;

        private StatusThread[] statusOfComputingThreads;
        private Thread[] computingThreads;
        private byte numberOfComputingThreads = 4;
        private string[] problemType = { "DVRP", "TSP" };

        private CommunicationModule communicationModule;
        private ulong NodeId { get; set; }
        private TimeSpan Timeout { get; set; }
        private Socket socket;

        private List<SolutionsMessage> solutionsMessages;

        public ComputationnalNode(string serverIp, int serverPort, int timeout)
        {
            this.communicationModule = new CommunicationModule(serverIp, serverPort, timeout);
            this.CreateProcessingThreads();
            this.partialProblemsQueue = new Queue<PartialProblemsMessage>();
            this.partialSolutionsQueue = new Queue<SolutionsMessage>();

            this.solutionsMessages = new List<SolutionsMessage>();
        }

        private void CreateProcessingThreads()
        {
            this.computingThreads = new Thread[this.numberOfComputingThreads];
            this.statusOfComputingThreads = new StatusThread[this.numberOfComputingThreads];
            for (int i = 0; i < this.numberOfComputingThreads; ++i)
            {
                this.statusOfComputingThreads[i] = new StatusThread()
                {
                    HowLong = 0,
                    ProblemInstanceId = 0,
                    ProblemType = String.Empty,
                    State = StatusThreadState.Idle,
                    TaskId = 0,
                    TaskIdSpecified = false
                };
            }
        }

        public bool RegisterAtServer()
        {
            //Connect to CS
            this.socket = this.communicationModule.SetupClient();
            this.communicationModule.Connect(socket);
            Trace.WriteLine("Connected to CS");

            //Create RegisterMessage
            var registerMessage = new RegisterMessage()
            {
                ParallelThreads = this.numberOfComputingThreads,
                SolvableProblems = this.problemType,
                Type = RegisterType.ComputationalNode
            };

            //Send RegisterMessage to CS
            var messageString = SerializeMessage(registerMessage);
            this.communicationModule.SendData(messageString, socket);
            Trace.WriteLine("Sent data to CS: " + messageString);

            //Receive RegisterResponseMessage from CS
            var receivedMessage = this.communicationModule.ReceiveData(socket);
            Trace.WriteLine("Received data from CS: " + receivedMessage);
            this.communicationModule.CloseSocket(socket);

            var deserializedMessage = DeserializeMessage<RegisterResponseMessage>(receivedMessage);

            //Register succeed
            if (deserializedMessage is RegisterResponseMessage)
            {
                Trace.WriteLine("Register at CS succeed");
                this.NodeId = deserializedMessage.Id;
                this.Timeout = deserializedMessage.TimeoutTimeSpan;
                return true;
            }

            //Register failed
            else
            {
                Trace.WriteLine("Register at CS failed");
                return false;
            }
        }

        public void StartSendingStatusThread()
        {
            this.sendingStatusThreadCancellationTokenSource = new CancellationTokenSource();
            this.sendingStatusThread = new Thread(this.SendStatusMessage);

            //Start sending StatusMessage
            this.sendingStatusThread.Start();
        }

        public void StartQueueProcessingThread()
        {
            this.processingThreadCancellationTokenSource = new CancellationTokenSource();
            this.processingQueueThread = new Thread(this.ProcessingThread);

            //Start processing PartialProblems Queue
            this.processingQueueThread.Start();
        }

        private void SendStatusMessage()
        {
            while (!sendingStatusThreadCancellationTokenSource.IsCancellationRequested)
            {
                //Connect to CS
                socket = communicationModule.SetupClient();
                communicationModule.Connect(socket);
                Trace.WriteLine("Connected to CS");

                //Create StatusMessage
                var statusMessage = new StatusMessage()
                {
                    Id = this.NodeId,
                    Threads = this.statusOfComputingThreads
                };

                //Send StatusMessage to CS
                var messageString = this.SerializeMessage(statusMessage);
                this.communicationModule.SendData(messageString, socket);
                Trace.WriteLine("Sent data to CS: " + messageString);

                //Receive any response from CS
                var receivedMessage = communicationModule.ReceiveData(socket);

                //Close connection
                this.communicationModule.CloseSocket(socket);

                if (receivedMessage != String.Empty)
                {
                    Trace.WriteLine("Received data from CS: " + receivedMessage);

                    //Received SolvePartialProblems message from CS
                    if (GetMessageName(receivedMessage) == "SolvePartialProblems")
                    {
                        Trace.WriteLine("SolvePartialProblems message recognized");
                        this.ProcessSolvePartialProblemsMessage(receivedMessage);
                    }

                    //Received unrecoginized message from CS
                    else
                    {
                        Trace.WriteLine("Received unrecognized message!");
                    }
                }
                else
                {
                    Trace.WriteLine("No received data from CS!");
                }

                //Sleep for the period of time given by CS
                Thread.Sleep(this.Timeout);
            }
        }

        private void ProcessSolvePartialProblemsMessage(string receivedMessage)
        {
            var partialProblemMessage = this.DeserializeMessage<PartialProblemsMessage>(receivedMessage);

            //Add PartialProblem to queue, it will be process by another Thread
            lock (this.partialProblemsQueue)
            {
                this.partialProblemsQueue.Enqueue(partialProblemMessage);
                Trace.WriteLine(partialProblemMessage.Id, "PartialProblem id[{0}] added to queue");
            }
        }

        private void ProcessingThread()
        {
            while (!this.processingThreadCancellationTokenSource.IsCancellationRequested)
            {
                this.ProcessPartialProblemMessage();

                this.ProcessSolutionsMessage();

                //Sleep for the period of time given by CS
                Thread.Sleep(this.Timeout);
            }
        }

        private void ProcessPartialProblemMessage()
        {
            PartialProblemsMessage partialProblemMessage = null;

            lock (this.partialProblemsQueue)
            {
                //If there is PartialProblem waiting in queue, dequeue
                if (this.partialProblemsQueue.Count > 0)
                {
                    partialProblemMessage = this.partialProblemsQueue.Dequeue();
                    Trace.WriteLine(partialProblemMessage.Id, "PartialProblem id[{0}] removed from queue");

                    Thread solvePartialProblemThread = new Thread(() => SolvePartialProblem(partialProblemMessage));
                    solvePartialProblemThread.Start();
                }

                //If there is NOT PartialProblem waiting in queue, return
                else
                {
                    Trace.WriteLine("No PartialProblem in queue");
                }
            }
        }

        private void ProcessSolutionsMessage()
        {
            //TODO wysyłanie gotowych solucji do serwera
        }

        private void SolvePartialProblem(PartialProblemsMessage partialProblemMessage)
        {
            //All PartialProblems which were sent in single PartialProblemsMessage
            Queue<SolvePartialProblemsPartialProblem> q = new Queue<SolvePartialProblemsPartialProblem>(partialProblemMessage.PartialProblems);

            TaskSolverDVRP taskSolverDvrp = new TaskSolverDVRP(partialProblemMessage.CommonData);

            SolutionsMessage solutionMessage = new SolutionsMessage()
                {
                    CommonData = partialProblemMessage.CommonData,
                    Id = partialProblemMessage.Id,
                    ProblemType = partialProblemMessage.ProblemType,
                    Solutions = new Solution[q.Count]
                };
            this.solutionsMessages.Add(solutionMessage);

            int idleThreadIndex;
            while (q.Count > 0)
            {
                lock (this.statusOfComputingThreads)
                {
                    //Get index number of the idle thread, which can compute
                    idleThreadIndex = this.GetIdleThreadIndex();
                }

                //If there is no idle thread, wait and try again
                if (idleThreadIndex == -1)
                {
                    Thread.Sleep(this.Timeout);
                    continue;
                }

                else
                {
                    this.statusOfComputingThreads[idleThreadIndex].ProblemInstanceId =
                        partialProblemMessage.Id;

                    this.statusOfComputingThreads[idleThreadIndex].ProblemType =
                        partialProblemMessage.ProblemType;

                    this.statusOfComputingThreads[idleThreadIndex].TaskId =
                        q.First().TaskId;

                    this.computingThreads[idleThreadIndex] = new Thread(() => 
                        this.Solve(q.Dequeue(), idleThreadIndex, taskSolverDvrp, 
                        (int)partialProblemMessage.SolvingTimeout));

                    this.computingThreads[idleThreadIndex].Start();
                }
            }
        }

        private void Solve(SolvePartialProblemsPartialProblem partialProblem, int threadNumber, TaskSolverDVRP taskSolverDvrp, int solvingTimeout)
        {
            //TODO Timer dla wątku do StatusThread.HowLong

            Solution solution = new Solution();
            solution.Data = taskSolverDvrp.Solve(partialProblem.Data, new TimeSpan(0,0,solvingTimeout));

            //TODO dodawanie dla właściwej SolutionMessage na liście
        }

        private int GetIdleThreadIndex()
        {
            //Find an idle Thread to process partial problem
            for (int i = 0; i < this.numberOfComputingThreads; ++i)
            {
                if (this.statusOfComputingThreads[i].State == StatusThreadState.Idle)
                {
                    this.statusOfComputingThreads[i].State = StatusThreadState.Busy;
                    return i;
                }
            }

            //If there is no idle computing thread
            return -1;
        }
    }


}
