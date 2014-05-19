using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Communication_Library;
using log4net;
using log4net.Appender;

namespace Computational_Server
{
    [MethodBoundary]
    public class ComputationServer : BaseNode
    {
        private Thread listeningThread;
        private Thread processingThread;
        private CancellationTokenSource listeningThreadCancellationTokenSource;
        private CancellationTokenSource processingThreadCancellationToken;

        private ConcurrentDictionary<ulong, SolveRequestMessage> solveRequests;
        private List<PartialProblemsMessage> dividedProblems; 
        private Dictionary<ulong, NodeEntry> activeNodes;
        private List<SolutionsMessage> partialSolutions;
        private List<SolutionsMessage> finalSolutions;
        private List<PartialProblemsMessage> partialProblems;
        private ICommunicationModule communicationModule;
        public readonly int processingThreadSleepTime;
        public readonly TimeSpan DefaultTimeout;
        private ulong nodesId;
        private object nodesIdLock = new object();
        private Socket serverSocket;
        private ulong solutionId;
        private object solutionIdLock = new object();
        MethodInfo serializeMessageMethod;

        public ComputationServer(TimeSpan nodeTimeout, ICommunicationModule communicationModule, int threadSleepTime)
        {
            solveRequests = new ConcurrentDictionary<ulong, SolveRequestMessage>();
            activeNodes = new Dictionary<ulong, NodeEntry>();
            finalSolutions = new List<SolutionsMessage>();
            partialSolutions = new List<SolutionsMessage>();
            dividedProblems = new List<PartialProblemsMessage>();
            DefaultTimeout = nodeTimeout;
            nodesId = 1;
            solutionId = 1;
            this.communicationModule = communicationModule;
            this.processingThreadSleepTime = threadSleepTime;
            partialProblems = new List<PartialProblemsMessage>();
            serializeMessageMethod = typeof(ComputationServer).GetMethod("SerializeMessage");
        }

        public void StartServer()
        {
            StartListeningThread();
            StartProcessingThread();
        }

        private void StartListeningThread()
        {
            listeningThreadCancellationTokenSource = new CancellationTokenSource();
            listeningThread = new Thread(ListeningThread);
            listeningThread.Start();
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
                RemoveUnusedNodes();

                Thread.Sleep(processingThreadSleepTime);
            }
        }

        private void RemoveUnusedNodes()
        {
            lock (activeNodes)
            {
                var nodesToDelete = activeNodes.Where(x => HasNodeExpired(x.Value)).ToList();
                for (int i = 0; i < nodesToDelete.Count; i++)
                {
                    var nodeToDelete = nodesToDelete[i];
                    NodeEntry deletedNode = null;
                    lock (activeNodes)
                    {
                        if (!activeNodes.Remove(nodeToDelete.Key))
                            _logger.Error("Could not remove node from activeNodes list. NodeId: " + nodeToDelete.Key);
                    }
                    _logger.Debug("Removed node from activeNodes list. NodeId: " + nodeToDelete.Key);
                }
            }
        }

        private bool HasNodeExpired(NodeEntry x)
        {
            return (DateTime.Now - x.LastStatusSentTime) > DefaultTimeout;
        }

        public void ListeningThread()
        {
            serverSocket = communicationModule.SetupServer();
            while (!listeningThreadCancellationTokenSource.IsCancellationRequested)
            {
                var clientSocket = communicationModule.Accept(serverSocket);

                var receivedMessage = communicationModule.ReceiveData(clientSocket);

                string result = String.Empty;
                if (!String.IsNullOrEmpty(receivedMessage))
                    result = ProcessMessage(receivedMessage);

                if(!String.IsNullOrEmpty(result))
                    communicationModule.SendData(result, clientSocket);

                communicationModule.CloseSocket(clientSocket);
            }
        }

        private string ProcessMessage(string message)
        {
            var messageName = this.GetMessageName(message);
            switch (messageName)
            {
                case "Register":
                    return this.ProcessCaseRegister(message);

                case "SolveRequest":
                    return this.ProcessCaseSolveRequest(message);

                case "SolutionRequest":
                    return this.ProcessCaseSolutionRequest(message);

                case "Status":
                    return this.ProcessCaseStatus(message);

                case "SolvePartialProblems":
                    return this.ProcessCaseSolvePartialProblems(message);

                case "Solutions":
                    return this.ProcessCaseSolutions(message);
                    
                default:
                    break;
            }
            return String.Empty;
        }

        private string ProcessCaseRegister(string message)
        {
            var registerMessage = DeserializeMessage<RegisterMessage>(message);

            var newId = GenerateNewNodeId();

            RegisterNode(newId, registerMessage.Type, registerMessage.SolvableProblems.ToList(),
                registerMessage.ParallelThreads);

            var registerResponse = new RegisterResponseMessage()
            {
                Id = newId,
                TimeoutTimeSpan = this.DefaultTimeout
            };

            return SerializeMessage(registerResponse);
        }

        private ulong GenerateNewNodeId()
        {
            ulong newNodeId = 0;
            lock (nodesIdLock)
            {
                newNodeId = nodesId;
                this.nodesId++;
            }
            return newNodeId;
        }

        /// <summary>
        /// Registers node in server and adds it to active nodes queue
        /// </summary>
        /// <param name="newId"></param>
        /// <param name="type"></param>
        /// <param name="solvableProblems"></param>
        /// <param name="parallelThreads"></param>
        /// <returns>0 if success, negative value if there was an error</returns>
        private void RegisterNode(ulong newId, RegisterType type, List<string> solvableProblems, byte parallelThreads)
        {
            var node = new NodeEntry(newId, type, solvableProblems, parallelThreads);
            lock (activeNodes)
            {
                activeNodes.Add(newId, node);
                _logger.Debug("Node added to server list");
            }
        }

        private string ProcessCaseSolutions(string message)
        {
            var deserializedMessage = DeserializeMessage<SolutionsMessage>(message);

            SolutionsMessage oldSolutions = null;
            if (IsFinal(deserializedMessage))
            {
                lock (finalSolutions)
                {
                    finalSolutions.Add(deserializedMessage);
                    _logger.Debug("--------------Added final solution to queue------------");
                    return String.Empty;
                }
            }
            else
            {
                lock (partialSolutions)
                {
                    oldSolutions = partialSolutions.FirstOrDefault(x => x.Id == deserializedMessage.Id);
                    if(oldSolutions == null)
                    {
                        partialSolutions.Add(deserializedMessage);
                        return String.Empty;
                    }
                    partialSolutions.Remove(oldSolutions);
                    oldSolutions = MergeSolutions(oldSolutions, deserializedMessage);
                    partialSolutions.Add(oldSolutions);
                }
            }
            return string.Empty;
        }

        private bool IsFinal(SolutionsMessage solutionsMessage)
        {
            bool final = true;
            for (int i = 0; i < solutionsMessage.Solutions.Length; i++)
            {
                var solution = solutionsMessage.Solutions[i];
                if (solution.Type != SolutionType.Final)
                    final = false;
            }
            return final;
        }

        private bool AllPartialSolutionSolved(SolutionsMessage oldSolutions)
        {
            bool solved = true;
            if (oldSolutions == null)
                return false;
            for (int i = 0; i < oldSolutions.Solutions.Length; i++)
            {
                var solution = oldSolutions.Solutions[i];
                if (solution.Type != SolutionType.Partial)
                    solved = false;
            }
            return solved;
        }

        private string ProcessCaseSolvePartialProblems(string message)
        {
            var deserializedMessage = DeserializeMessage<PartialProblemsMessage>(message);
            lock (dividedProblems)
            {
                dividedProblems.Add(deserializedMessage);
            }
            var partialProblemsDivided = DividePartialProblems(deserializedMessage);
            lock (partialProblems)
            {
                for (int i = 0; i < partialProblemsDivided.Count; i++)
                {
                    var partialProblem = partialProblemsDivided[i];
                    partialProblems.Add(partialProblem);
                }
            }
            return string.Empty;
        }

        private List<PartialProblemsMessage> DividePartialProblems(PartialProblemsMessage partialProblemsMessage)
        {
            var partialProblemsList = new List<PartialProblemsMessage>();
            for (int i = 0; i < partialProblemsMessage.PartialProblems.Length; i++)
            {
                var partialProblem = new PartialProblemsMessage()
                {
                    Id = partialProblemsMessage.Id,
                    CommonData = partialProblemsMessage.CommonData,
                    ProblemType = partialProblemsMessage.ProblemType,
                    PartialProblems =
                        new SolvePartialProblemsPartialProblem[1] {partialProblemsMessage.PartialProblems[i]},
                    SolvingTimeout = partialProblemsMessage.SolvingTimeout,
                    SolvingTimeoutSpecified = partialProblemsMessage.SolvingTimeoutSpecified
                };
                partialProblemsList.Add(partialProblem);
            }
            return partialProblemsList;
        }

        private SolutionsMessage MergeSolutions(SolutionsMessage oldSolutionsMessage, SolutionsMessage newSolutionsMessage)
        {
            for (int i = 0; i < newSolutionsMessage.Solutions.Length; i++)
            {
                var newSolution = newSolutionsMessage.Solutions[i];
                if(newSolution == null)
                    throw new Exception("Could solutions is null " + newSolution.TaskId + ", problemId: " + newSolutionsMessage.Id);

                var oldSolutions = oldSolutionsMessage.Solutions.ToList();
                oldSolutions.AddRange(newSolutionsMessage.Solutions);
                oldSolutionsMessage.Solutions = oldSolutions.ToArray();
            }
            return oldSolutionsMessage;
        }

        private string ProcessCaseStatus(string message)
        {
            var deserializedStatusMessage = DeserializeMessage<StatusMessage>(message);
            _logger.Info("Received status from nodeId: " + deserializedStatusMessage.Id);
            
            UpdateNodesLifetime(deserializedStatusMessage);

            UpdateSolutionsStatus(deserializedStatusMessage);

            var node = GetActiveNode(deserializedStatusMessage.Id);
            if (node == null)
                return String.Empty;
            var nodeTask = GetTaskForNode(node);
            if (nodeTask == null)
                return String.Empty;

            var declaringType = nodeTask.GetType();
            
            MethodInfo generic = serializeMessageMethod.MakeGenericMethod(declaringType);

            return (string)generic.Invoke(this, new object[] { nodeTask });
        }

        private void UpdateSolutionsStatus(StatusMessage statusMessage)
        {
            //foreach (var thread in statusMessage.Threads.Where(x => x.State == StatusThreadState.Busy))
            //{
            //    SolutionsMessage solution = null;
            //    lock (partialSolutions)
            //    {
            //        solution = partialSolutions.FirstOrDefault(x => x.Id == thread.ProblemInstanceId);
            //        var subSolution = solution.Solutions.FirstOrDefault(x => x.TaskId == thread.TaskId);
            //        subSolution.ComputationsTime += thread.HowLong;
            //    }
            //}
        }

        private ComputationMessage GetTaskForNode(NodeEntry node)
        {
            switch (node.Type)
            {
                case RegisterType.TaskManager:
                    return GetTaskForTaskManager(node);
                case RegisterType.ComputationalNode:
                    return GetTaskForComputationalNode(node);
                default:
                    _logger.Error("GetTaskForNode error: Unknown node type");
                    return null;
            }
        }

        private ComputationMessage GetTaskForComputationalNode(NodeEntry node)
        {
            PartialProblemsMessage partialProblem = null;
            lock (partialProblems)
            {
                partialProblem = partialProblems.FirstOrDefault(x => node.SolvingProblems.Contains(x.ProblemType));
                if (partialProblem != null)
                    partialProblems.Remove(partialProblem);
            }
            return partialProblem;
        }

        private ComputationMessage GetTaskForTaskManager(NodeEntry node)
        {
            var divideMessage = GetDivideProblemMessageForType(node.SolvingProblems);
            if (divideMessage != null)
                return divideMessage;
            var partialSolutionsMessage = GetPartialSolutionForType(node.Type);
            if (partialSolutionsMessage != null)
                return partialSolutionsMessage;
            return null;
        }

        /// <summary>
        /// Returns SolutionsMessage with all partial solutions solved for merging
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private SolutionsMessage GetPartialSolutionForType(RegisterType type)
        {
            SolutionsMessage partialSolution = null;
            lock (partialSolutions)
            {
                foreach (var incompleteSolution in partialSolutions)
                {
                    lock (dividedProblems)
                    {
                        var partialProblem = dividedProblems.FirstOrDefault(x => x.Id == incompleteSolution.Id);
                        if (partialProblem == null)
                            continue;
                        if (partialProblem.PartialProblems.Length == incompleteSolution.Solutions.Length)
                            partialSolution = incompleteSolution; // found complete solution
                    }
                }
                if (partialSolution != null)
                {
                    partialSolutions.Remove(partialSolution);
                }
            }
            return partialSolution;
        }

        /// <summary>
        /// Dequeues first solveReqest of given type and returns appropiate DivideProblemMessage for SolveRequest
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private DivideProblemMessage GetDivideProblemMessageForType(List<string> solvingTypes)
        {
            DivideProblemMessage divideProblemMessage = null;
            ulong solveRequestId = 0;
            SolveRequestMessage solveRequest = new SolveRequestMessage();
            lock(solveRequests)
            {
                solveRequestId = solveRequests.FirstOrDefault(x => solvingTypes.Contains(x.Value.ProblemType)).Key;
                solveRequests.TryRemove(solveRequestId, out solveRequest);
            }

            if (solveRequest != null)
            {
                int activeNodeCount = 0;
                
                lock (activeNodes)
                {
                    foreach (var activeNode in this.activeNodes)
                    {
                        if (activeNode.Value.Type == RegisterType.ComputationalNode) 
                            activeNodeCount++;
                    }
                }

                divideProblemMessage = new DivideProblemMessage()
                {
                    ComputationalNodes = (ulong)activeNodeCount,
                    Data = solveRequest.Data,
                    ProblemType = solveRequest.ProblemType,
                    Id = solveRequestId
                };
            }
            
            return divideProblemMessage;
        }

        private NodeEntry GetActiveNode(ulong nodeId)
        {
            NodeEntry node = null;
            if (!activeNodes.TryGetValue(nodeId, out node))
            {
                string errorMessage = "Could not get value of nodeId: " + nodeId + " from dictionary.";
                _logger.Error(errorMessage);
            }
            return node;
        }

        private void UpdateNodesLifetime(StatusMessage statusMessage)
        {
            try
            {
                lock (activeNodes)
                {
                    NodeEntry node = null;
                    lock (activeNodes)
                    {
                        node = activeNodes[statusMessage.Id];
                        if (node == null)
                        {
                            _logger.Error("Error updating node lifetime. Could not find node: " + statusMessage.Id);
                            return;
                        }
                        node.LastStatusSentTime = DateTime.Now;
                        _logger.Debug("Updated node lifetime. Nodeid: " + statusMessage.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Could not update nodes lifetime. NodeId: " + statusMessage.Id + ". Exception: " + ex.ToString());
            }
        }

        private string ProcessCaseSolutionRequest(string message)
        {
            var deserializedMessage = DeserializeMessage<SolutionRequestMessage>(message);
            SolutionsMessage solution = null;

            lock (finalSolutions)
            {
                lock (dividedProblems)
                {
                    solution = finalSolutions.FirstOrDefault(x => x.Id == deserializedMessage.Id);
                    var partialProblemsToRemove = dividedProblems.FirstOrDefault(x => x.Id == deserializedMessage.Id);

                    if (solution != null && partialProblemsToRemove != null)
                    {
                        finalSolutions.Remove(solution);
                        dividedProblems.Remove(partialProblemsToRemove);
                    }
                }
            }

            if (solution == null)
                return String.Empty;
            
            return SerializeMessage<SolutionsMessage>(solution);
        }

        private string ProcessCaseSolveRequest(string message)
        {
            var deserializedMessage = DeserializeMessage<SolveRequestMessage>(message);
            
            ulong solutionId = GenerateNewSolutionId();

            var solveRequestResponse = new SolveRequestResponseMessage() { Id = solutionId };
            if (!solveRequests.TryAdd(solutionId, deserializedMessage))
            {
                _logger.Error("Could not add SolveRequest to dictionary. SolutionId: " + solutionId + ", message: " + deserializedMessage);
                solveRequestResponse.Id = 0;
            }
            
            return SerializeMessage(solveRequestResponse);
        }

        private ulong GenerateNewSolutionId()
        {
            ulong newSolutionId = 0;
            lock (solutionIdLock)
            {
                newSolutionId = solutionId;
                solutionId++;
            }
            return newSolutionId;
        }

        public void StopServer()
        {
            listeningThreadCancellationTokenSource.Cancel();
            listeningThread.Join();
            _logger.Info("Stopped listening");
        }
    }
}