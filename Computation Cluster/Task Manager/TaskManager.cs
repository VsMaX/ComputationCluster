using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Communication_Library;
using log4net;
using UCCTaskSolver;
using System.Threading;
using System.Xml;
using DynamicVehicleRoutingProblem;
using System.Collections.Concurrent;

namespace Task_Manager
{
    [MethodBoundary]
    public class TaskManager : BaseNode
    {
        private int serverPort;
        private string serverIp;
        private ICommunicationModule communicationModule;
        public ulong NodeId { get; set; }
        public TimeSpan Timeout { get; set; }
        public int NumberOfThreads { get; set; }

        private Thread statusThread;
        private Thread processingThread;
        private CancellationTokenSource statusThreadCancellationTokenSource;
        private CancellationTokenSource processingThreadCancellationToken;
        private DateTime startTime;
        private StatusThreadState state;
        private Queue<DivideProblemMessage> divideProblemMessageQueue;
        private Queue<SolutionsMessage> partialSolutionsMessageQueue;
        public List<TaskSolver> TaskSolvers { get; set; }
        private ulong problemId;
        private object problemIdLock = new object();
        private int threadSleepTimeout;

        public TaskManager(string serverIp, int serverPort, int receiveDataTimeout, int threadSleepTimeout)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort, receiveDataTimeout);
            startTime = DateTime.Now;
            divideProblemMessageQueue = new Queue<DivideProblemMessage>();
            partialSolutionsMessageQueue = new Queue<SolutionsMessage>();
            this.threadSleepTimeout = threadSleepTimeout;
        }

        public void Start()
        {
            RegisterAtServer();
            StartStatusThread();
            StartProcessingThread();
            _logger.Info("Starting TM");
        }

        public void RegisterAtServer()
        {
            var registerMessage = new RegisterMessage()
            {
                ParallelThreads = 1,//???
                SolvableProblems = new string[] { "DVRP" },
                Type = RegisterType.TaskManager
            };
            var messageString = SerializeMessage(registerMessage);

            var socket = communicationModule.SetupClient();
            communicationModule.Connect(socket);
            communicationModule.SendData(messageString, socket);

            var response = communicationModule.ReceiveData(socket);
            var deserializedResponse = DeserializeMessage<RegisterResponseMessage>(response);
            this.NodeId = deserializedResponse.Id;
            this.Timeout = deserializedResponse.TimeoutTimeSpan;

            communicationModule.CloseSocket(socket);
            _logger.Info("Succesfully registered at server. Assigned id: " + this.NodeId + " received timeout: " +
                         deserializedResponse.Timeout);
        }

        private void StartStatusThread()
        {
            statusThreadCancellationTokenSource = new CancellationTokenSource();
            statusThread = new Thread(SendStatusThread);
            statusThread.Start();
        }

        private void StartProcessingThread()
        {
            processingThreadCancellationToken = new CancellationTokenSource();
            processingThread = new Thread(ProcessingThread);
            processingThread.Start();
        }

        private void ProcessingThread()
        {
            while (!processingThreadCancellationToken.IsCancellationRequested)
            {
                ProcessDivideProblem();

                ProcessPartialSolutions();
                Thread.Sleep(threadSleepTimeout);
            }
        }

        private void ProcessPartialSolutions()
        {
            SolutionsMessage solution = null;
            lock (partialSolutionsMessageQueue)
            {
                if(partialSolutionsMessageQueue.Count > 0)
                    solution = partialSolutionsMessageQueue.Dequeue();
            }

            if (solution == null)
                return;

            _logger.Debug("Merging solution");

            TaskSolverDVRP taskSolver = CreateTaskSolver(solution.ProblemType, solution.CommonData);

            taskSolver.MergeSolution(solution.Solutions.Select(x => x.Data).ToArray());

            _logger.Debug("Finished merging solution: " + solution.Id);
            _logger.Debug("Finished merging solution: " + solution.Id);
            _logger.Debug("Finished merging solution: " + solution.Id);

            SolutionsMessage solutionsMessage = new SolutionsMessage()
            {
                Id = solution.Id,
                CommonData = null,
                ProblemType = "DVRP",
                Solutions = new Solution[1]
            };
            solutionsMessage.Solutions[0] = new Solution()
            {
                Type = SolutionType.Final,
                Data = taskSolver.Solution
            };

            var socket = communicationModule.SetupClient();
            communicationModule.Connect(socket);
            var message = SerializeMessage(solutionsMessage);
            communicationModule.SendData(message, socket);

            communicationModule.CloseSocket(socket);
        }

        private void ProcessDivideProblem()
        {
            DivideProblemMessage processedMessage = null;
            lock (divideProblemMessageQueue)
            {
                if(divideProblemMessageQueue.Count > 0)
                    processedMessage = divideProblemMessageQueue.Dequeue();
            }
            if (processedMessage == null)
                return;
            
            _logger.Debug("Processing divideMessage. " + processedMessage.Id);

            TaskSolverDVRP taskSolver = CreateTaskSolver(processedMessage.ProblemType, processedMessage.Data);

            var dividedProblem = taskSolver.DivideProblem(3);

            _logger.Debug("Finished dividing problem");


            var solutionsMessage = new PartialProblemsMessage()
            {
                CommonData = processedMessage.Data,
                Id = processedMessage.Id,
                ProblemType = processedMessage.ProblemType,
                SolvingTimeout = (ulong) Timeout.TotalMilliseconds,
                SolvingTimeoutSpecified = true,
                PartialProblems = new SolvePartialProblemsPartialProblem[(int) processedMessage.ComputationalNodes]
            };

            solutionsMessage.PartialProblems = new SolvePartialProblemsPartialProblem[dividedProblem.Length];

            for (int i = 0; i < dividedProblem.Length; i++)
            {
                solutionsMessage.PartialProblems[i] = new SolvePartialProblemsPartialProblem()
                {
                    Data = dividedProblem[i],
                    TaskId = (ulong)i
                };
            }

            var socket = communicationModule.SetupClient();
            communicationModule.Connect(socket);
            var message = SerializeMessage(solutionsMessage);
            communicationModule.SendData(message, socket);
            communicationModule.CloseSocket(socket);
        }

        private ulong GetProblemId()
        {
            ulong problemIdTmp = 0;

            lock (problemIdLock)
            {
                problemIdTmp = problemId;
                problemId++;
            }
            return problemIdTmp;
        }

        private TaskSolverDVRP CreateTaskSolver(string problemType, byte[] data)
        {
            return new TaskSolverDVRP(data);
        }

        public void SendStatusThread()
        {
            while (!statusThreadCancellationTokenSource.IsCancellationRequested)
            {
                var socket = communicationModule.SetupClient();
                communicationModule.Connect(socket);

                var st= new StatusThread()
                {
                    HowLong = (ulong)(DateTime.Now-startTime).TotalMilliseconds, 
                    TaskId = 1,
                    State = StatusThreadState.Idle, 
                    ProblemType = "DVRP", 
                    ProblemInstanceId = 1, 
                    TaskIdSpecified = true
                };
                var statusMessage = new StatusMessage(this.NodeId, new StatusThread[] { st });
                var statusMessageString = SerializeMessage(statusMessage);
                communicationModule.SendData(statusMessageString, socket);
                var receivedMessage = communicationModule.ReceiveData(socket);
                
                communicationModule.CloseSocket(socket);

                if(!String.IsNullOrEmpty(receivedMessage))
                    ProcessMessage(receivedMessage);

                Thread.Sleep((int)(threadSleepTimeout * 0.8));
            }
        }

        private string ProcessMessage(string message)
        {
            var messageName = this.GetMessageName(message);
            switch (messageName)
            {
                case "DivideProblem":
                    ProcessCaseDivideProblem(message);
                    break;
                case "Solutions":
                    ProcessCasePartialSolutions(message);
                    break;
                default:
                    break;
            }
            return String.Empty;
        }

        private void ProcessCaseDivideProblem(string message)
        {
            var deserializedDivideProblemMessage = DeserializeMessage<DivideProblemMessage>(message);
            lock (divideProblemMessageQueue)
            {
                divideProblemMessageQueue.Enqueue(deserializedDivideProblemMessage);
            }
        }

        private void ProcessCasePartialSolutions(string message)
        {
            var deserializedPartialSolutionsMessage = DeserializeMessage<SolutionsMessage>(message);
            lock (partialSolutionsMessageQueue)
            {
                partialSolutionsMessageQueue.Enqueue(deserializedPartialSolutionsMessage);
                _logger.Debug("Enqueued partial solution");
            }
        }

        public void Stop()
        {
            statusThreadCancellationTokenSource.Cancel();
            statusThread.Join();
            processingThreadCancellationToken.Cancel();
            processingThread.Join();
            _logger.Info("TaskManager stopped");
        }
    }
}
