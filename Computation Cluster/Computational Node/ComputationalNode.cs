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
        private Queue<Solution> partialSolutionsQueuePart;

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
            this.partialSolutionsQueuePart = new Queue<Solution>();

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
                //Trace.WriteLine("Connected to CS");

                //Create StatusMessage
                var statusMessage = new StatusMessage()
                {
                    Id = this.NodeId,
                    Threads = this.statusOfComputingThreads
                };

                //Send StatusMessage to CS
                var messageString = this.SerializeMessage(statusMessage);
                this.communicationModule.SendData(messageString, socket);
                //Trace.WriteLine("Sent data to CS: " + messageString);

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
                Thread.Sleep(4000);
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
                Thread.Sleep(4000);
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
                    Trace.WriteLine("Starting solving problem id: " + partialProblemMessage.Id);
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
            lock(this.partialSolutionsQueue)
            {
                //if(this.partialSolutionsQueue.Count > 0)
                //{
                //    SolutionsMessage solutionsMessage = this.partialSolutionsQueue.Dequeue();

                //    //Connect to CS
                //    socket = communicationModule.SetupClient();
                //    communicationModule.Connect(socket);

                //    //Send SolutionsMessage to CS
                //    var messageString = this.SerializeMessage(solutionsMessage);
                //    this.communicationModule.SendData(messageString, socket);

                //    //Close connection
                //    this.communicationModule.CloseSocket(socket);
                //}
                int i = 0;
                int counter = 0;
                while(this.solutionsMessages.Count > 0)
                {

                    foreach (var solution in solutionsMessages[i].Solutions)
                    {
                        if(solution != null && solution.Type != SolutionType.Partial)
                        {
                            counter++;
                        }
                    }

                    if(counter == solutionsMessages[i].Solutions.Length)
                    {
                        //Connect to CS
                        socket = communicationModule.SetupClient();
                        communicationModule.Connect(socket);

                        //Send SolutionsMessage to CS
                        var messageString = this.SerializeMessage(this.solutionsMessages[i]);
                        this.communicationModule.SendData(messageString, socket);

                        //Close connection
                        this.communicationModule.CloseSocket(socket);

                        this.solutionsMessages.RemoveAt(i);
                    }
                    else
                    {
                        counter = 0;
                    }
                    
                }
            }
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
                    Solutions = new Solution[q.Count],
                };
            lock ((this.solutionsMessages))
            {
                this.solutionsMessages.Add(solutionMessage);
            }
            
            int idleThreadIndex;
            int counter = 0;
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
                    Thread.Sleep(4000);
                }

                else
                {
                    this.statusOfComputingThreads[idleThreadIndex].ProblemInstanceId =
                        partialProblemMessage.Id;

                    this.statusOfComputingThreads[idleThreadIndex].ProblemType =
                        partialProblemMessage.ProblemType;

                    this.statusOfComputingThreads[idleThreadIndex].TaskId =
                        q.Peek().TaskId;
                    var prob = q.Dequeue();
                    this.computingThreads[idleThreadIndex] = new Thread(() => 
                        this.Solve(prob, idleThreadIndex, taskSolverDvrp, 
                        (int)partialProblemMessage.SolvingTimeout, partialProblemMessage.Id, counter));

                    this.computingThreads[idleThreadIndex].Start();
                }
            }
        }

        private void Solve(SolvePartialProblemsPartialProblem partialProblem, int threadNumber, TaskSolverDVRP taskSolverDvrp, int solvingTimeout, ulong id, int index)
        {
            //TODO Timer dla wątku do StatusThread.HowLong

            Solution solution = new Solution()
                {
                    ComputationsTime = 0,
                    Data = null,
                    TaskId = partialProblem.TaskId,
                    TaskIdSpecified = (int)partialProblem.TaskId < 0 ? false : true,
                    TimeoutOccured = false,
                    Type = SolutionType.Ongoing
                };

            //solution.ComputationsTime = 
            solution.Data = taskSolverDvrp.Solve(partialProblem.Data, new TimeSpan(0,0,solvingTimeout));
            Trace.WriteLine("Solved problem: " + solution.TaskId);
            //solution.TimeoutOccured =
            solution.Type = SolutionType.Partial;

            lock (this.solutionsMessages)
            {
                for (int j = solutionsMessages.Count - 1; j >= 0; j--)
                {
                    var solutionsMessage = solutionsMessages[j];
                    if(solutionsMessage.Id == id)
                    {
                        solutionsMessage.Solutions[index] = solution;
                        break;
                    }
                    
                    //lock (this.partialSolutionsQueue)
                    //{
                    //    this.partialSolutionsQueue.Enqueue(solutionsMessage);
                    //}
                    //this.solutionsMessages.RemoveAt(j);
                }
            }

            lock (this.statusOfComputingThreads)
            {
                this.statusOfComputingThreads[threadNumber].State = StatusThreadState.Idle;
                this.statusOfComputingThreads[threadNumber].TaskIdSpecified = false;
            }
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
