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
using Plugin;

namespace Computational_Node
{
    [MethodBoundary]
    public class ComputationalNode : BaseNode
    {
        private Thread sendingStatusThread;
        private CancellationTokenSource sendingStatusThreadCancellationTokenSource;

        private Thread processingProblemsQueueThread;
        private CancellationTokenSource processingProblemsQueueThreadCancellationTokenSource;

        private Thread processingSolutionsQueueThread;
        private CancellationTokenSource processingSolutionsQueueThreadCancellationTokenSource;

        private Socket statusSocket;
        private Socket solutionsSocket;
        private Socket registerSocket;

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

        private List<SolutionsMessage> solutionsMessagesList;

        public ComputationalNode(string serverIp, int serverPort, int receivingTimeout)
        {
            _logger.Debug("Starting CN");
            this.communicationModule = new CommunicationModule(serverIp, serverPort, receivingTimeout);
            this.CreateProcessingThreads();
            this.partialProblemsQueue = new Queue<PartialProblemsMessage>();
            this.partialSolutionsQueue = new Queue<SolutionsMessage>();

            //
            this.partialSolutionsQueuePart = new Queue<Solution>();
            this.solutionsMessagesList = new List<SolutionsMessage>();
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
            this.registerSocket = this.communicationModule.SetupClient();
            this.communicationModule.Connect(registerSocket);
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
            this.communicationModule.SendData(messageString, registerSocket);
            Trace.WriteLine("Sent data to CS: " + messageString);

            //Receive RegisterResponseMessage from CS
            var receivedMessage = this.communicationModule.ReceiveData(registerSocket);
            Trace.WriteLine("Received data from CS: " + receivedMessage);
            this.communicationModule.CloseSocket(registerSocket);

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

        public void StartProblemsQueueProcessingThread()
        {
            this.processingProblemsQueueThreadCancellationTokenSource = new CancellationTokenSource();
            this.processingProblemsQueueThread = new Thread(this.ProblemsQueueProcessingThread);

            //Start processing PartialProblems Queue
            this.processingProblemsQueueThread.Start();
        }

        public void StartSolutionsQueueProcessingThread()
        {
            this.processingSolutionsQueueThreadCancellationTokenSource = new CancellationTokenSource();
            this.processingSolutionsQueueThread = new Thread(this.SolutionsQueueProcessingThread);

            //Start processing PartialSolutions Queue
            this.processingSolutionsQueueThread.Start();
        }

        private void SendStatusMessage()
        {
            while (!this.sendingStatusThreadCancellationTokenSource.IsCancellationRequested)
            {
                //Connect to CS
                statusSocket = communicationModule.SetupClient();
                communicationModule.Connect(statusSocket);

                //Create StatusMessage
                var statusMessage = new StatusMessage()
                {
                    Id = this.NodeId,
                    Threads = this.statusOfComputingThreads
                };

                //Send StatusMessage to CS
                var messageString = this.SerializeMessage(statusMessage);
                this.communicationModule.SendData(messageString, statusSocket);

                //Receive any response from CS
                var receivedMessage = communicationModule.ReceiveData(statusSocket);

                //Close connection
                this.communicationModule.CloseSocket(statusSocket);

                if (receivedMessage != String.Empty)
                {
                    Trace.WriteLine("Received data from CS: " + receivedMessage);

                    //Received SolvePartialProblems message from CS
                    if (GetMessageName(receivedMessage) == "SolvePartialProblems")
                    {
                        Trace.WriteLine("SolvePartialProblems message recognized");
                        this.AddPartialProblemToQueue(receivedMessage);
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

        private void AddPartialProblemToQueue(string receivedMessage)
        {
            var partialProblemMessage = this.DeserializeMessage<PartialProblemsMessage>(receivedMessage);

            //Add PartialProblem to queue, it will be process by processProblemsQueueThread
            lock (this.partialProblemsQueue)
            {
                this.partialProblemsQueue.Enqueue(partialProblemMessage);
                Trace.WriteLine( "PartialProblem added to queue");
            }
        }

        private void ProblemsQueueProcessingThread()
        {
            while (!this.processingProblemsQueueThreadCancellationTokenSource.IsCancellationRequested)
            {
                lock (this.partialProblemsQueue)
                {
                    if (this.partialProblemsQueue.Count > 0)
                    {
                        Trace.WriteLine("\n\n[" + this.partialProblemsQueue.Count + "] problems in queue\n\n");
                        this.SolvePartialProblemsMessage(this.partialProblemsQueue.Dequeue());
                    }
                }
                //Sleep for the period of time given by CS
                Thread.Sleep(4000);
            }
        }


        private void SolvePartialProblemsMessage(PartialProblemsMessage partialProblemsMessage)
        {
            Trace.WriteLine("\n\nStarting solving problem id: " + partialProblemsMessage.Id);

            //All PartialProblems which were sent in single PartialProblemsMessage
            Queue<SolvePartialProblemsPartialProblem> q = new Queue<SolvePartialProblemsPartialProblem>(partialProblemsMessage.PartialProblems);
            Trace.WriteLine("\n\nSOLVING "+ q.Count +" subproblems of PartialProblem\n\n");

            TaskSolverDVRP taskSolverDvrp = new TaskSolverDVRP(partialProblemsMessage.CommonData);

            SolutionsMessage solutionsMessage = new SolutionsMessage()
                {
                    CommonData = partialProblemsMessage.CommonData,
                    Id = partialProblemsMessage.Id,
                    ProblemType = partialProblemsMessage.ProblemType,
                    Solutions = new Solution[q.Count],
                };

            lock ((this.solutionsMessagesList))
            {
                this.solutionsMessagesList.Add(solutionsMessage);
            }

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
                    Thread.Sleep(4000);
                }

                else
                {
                    SolvePartialProblemsPartialProblem problem = q.Dequeue();

                    this.statusOfComputingThreads[idleThreadIndex].ProblemInstanceId =
                        partialProblemsMessage.Id;

                    this.statusOfComputingThreads[idleThreadIndex].ProblemType =
                        partialProblemsMessage.ProblemType;

                    this.statusOfComputingThreads[idleThreadIndex].TaskId =
                        problem.TaskId;

                    this.computingThreads[idleThreadIndex] = new Thread(() => 
                        this.Solve(problem, idleThreadIndex, taskSolverDvrp, 
                        (int)partialProblemsMessage.SolvingTimeout, partialProblemsMessage.Id));

                    this.computingThreads[idleThreadIndex].Start();
                }
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

        private void Solve(SolvePartialProblemsPartialProblem partialProblem, int threadNumber, TaskSolverDVRP taskSolverDvrp, int solvingTimeout, ulong id)
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

            Trace.WriteLine("\n\nSolving subproblem id " + partialProblem.TaskId + " on thread " + threadNumber + "\n\n");
            //solution.ComputationsTime = 
            solution.Data = taskSolverDvrp.Solve(partialProblem.Data, new TimeSpan(0, 0, solvingTimeout));
            //solution.TimeoutOccured =
            solution.Type = SolutionType.Partial;
            Trace.WriteLine("\n\nSolved subproblem id: " + partialProblem.TaskId + "\n\n");

            lock (this.solutionsMessagesList)
            {
                for (int i = 0; i < this.solutionsMessagesList.Count; ++i)
                {
                    if (this.solutionsMessagesList[i].Id == id)
                    {
                        for (int j = 0; j < this.solutionsMessagesList[i].Solutions.Length; j++)
                        {
                            if(this.solutionsMessagesList[i].Solutions[j] == null)
                                this.solutionsMessagesList[i].Solutions[j] = solution;
                        }
                        Trace.WriteLine("\n\n Subproblem id: " + partialProblem.TaskId + " waits for complete solution\n\n");
                        break;
                    }
                }
            }
            _logger.Debug("Solved problem: " + solution.TaskId);

            lock (this.statusOfComputingThreads)
            {
                this.statusOfComputingThreads[threadNumber].State = StatusThreadState.Idle;
            }
        }


        private void SolutionsQueueProcessingThread()
        {
            while (!this.processingSolutionsQueueThreadCancellationTokenSource.IsCancellationRequested)
            {
                lock (this.solutionsMessagesList)
                {
                    if (this.solutionsMessagesList.Count > 0)
                    {
                        this.SolutionsProcessingThread();
                    }
                }
                
                //Sleep for the period of time given by CS
                Thread.Sleep(4000);
            }
        }

        private void SolutionsProcessingThread()
        {
            int i = 0;
            int counter = 0;
            while (i < this.solutionsMessagesList.Count)
            {

                foreach (var solution in solutionsMessagesList[i].Solutions)
                {
                    if (solution != null && solution.Type == SolutionType.Partial)
                    {
                        counter++;
                    }
                }
                Trace.WriteLine("\n\n " + counter + "/" + solutionsMessagesList[i].Solutions.Length + " subproblems of partialProblem are solved");

                if (counter == solutionsMessagesList[i].Solutions.Length)
                {
                    Trace.WriteLine("\n\n ALL Subproblems of partialProblem ARE solved\n\n");

                    //Connect to CS
                    this.solutionsSocket = this.communicationModule.SetupClient();
                    this.communicationModule.Connect(solutionsSocket);

                    //Send SolutionsMessage to CS
                    var messageString = this.SerializeMessage(this.solutionsMessagesList[i]);
                    this.communicationModule.SendData(messageString, solutionsSocket);

                    //Close connection
                    this.communicationModule.CloseSocket(solutionsSocket);

                    this.solutionsMessagesList.RemoveAt(i);
                }
                else
                {
                    counter = 0;
                }
                i++;
            }
        }
    }
}
