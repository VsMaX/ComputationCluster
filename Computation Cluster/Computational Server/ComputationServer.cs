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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Communication_Library;
using log4net;

namespace Computational_Server
{
    public class ComputationServer : BaseNode
    {
        private Thread listeningThread;
        private Thread processingThread;
        private CancellationTokenSource listeningThreadCancellationTokenSource;
        private CancellationTokenSource processingThreadCancellationToken;

        private ConcurrentDictionary<ulong, SolveRequestMessage> solveRequests;
        private ConcurrentDictionary<ulong, NodeEntry> activeNodes;
        private ConcurrentBag<SolutionsMessage> partialSolutions;
        private ICommunicationModule communicationModule;
        public readonly int processingThreadSleepTime;
        public readonly TimeSpan DefaultTimeout;
        [DefaultValue(1)]
        private ulong nodesId;
        private object nodesIdLock = new object();
        private Socket serverSocket;
        [DefaultValue(1)]
        private ulong solutionId;
        private object solutionIdLock;

        public ComputationServer(TimeSpan nodeTimeout, ICommunicationModule communicationModule, int processingThreadSleepTime)
        {
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
                if (attr != null)
                {
                    prop.SetValue(this, attr.Value);
                }
            }
            solveRequests = new ConcurrentDictionary<ulong, SolveRequestMessage>();
            activeNodes = new ConcurrentDictionary<ulong, NodeEntry>();
            partialSolutions = new ConcurrentBag<SolutionsMessage>();
            DefaultTimeout = nodeTimeout;
            nodesId = 0;
            this.communicationModule = communicationModule;
            this.processingThreadSleepTime = processingThreadSleepTime;
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
                lock (activeNodes)
                {
                    var nodesToDelete = activeNodes.Where(x => HasNodeExpired(x.Value)).ToList();
                    for (int i = 0; i < nodesToDelete.Count; i++)
                    {
                        var nodeToDelete = nodesToDelete[i];
                        NodeEntry deletedNode = null;
                        if(!activeNodes.TryRemove(nodeToDelete.Key, out deletedNode))
                            _logger.Error("Could not remove node from activeNodes list. NodeId: " + nodeToDelete.Key);
                    }
                }
                Thread.Sleep(processingThreadSleepTime);
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
            _logger.Debug("Received " + messageName);
            _logger.Debug("XML Data: " + message);
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

                case "PartialProblems":
                    return this.ProcessCasePartialProblems(message);
                default:
                    break;
            }
            return String.Empty;
        }

        private string ProcessCasePartialProblems(string message)
        {
            throw new NotImplementedException();
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
                this.nodesId++;
                newNodeId = nodesId;
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
                activeNodes.TryAdd(newId, node);
                _logger.Debug("Node added to server list");
            }
        }

        private string ProcessCaseSolutions(string message)
        {
            var deserializedMessage = DeserializeMessage<SolutionsMessage>(message);

            //TODO Oczekiwanie na wlasciwego TM i przesłanie do niego poszczegolnych solucji

            return string.Empty;
        }

        private string ProcessCaseSolvePartialProblems(string message)
        {
            var deserializedMessage = DeserializeMessage<PartialProblemsMessage>(message);

            //TO DO Oczekiwanie na wlasciwe CN i przeslanie do nich podproblemow

            return string.Empty;
        }

        private string ProcessCaseStatus(string message)
        {
            string result = String.Empty;
            var deserializedStatusMessage = DeserializeMessage<StatusMessage>(message);
            _logger.Info("Received status from nodeId: " + deserializedStatusMessage.Id);
            UpdateNodesLifetime(deserializedStatusMessage);

            //TODO update nodes lifetime
            //TODO if status comes from TM, get all SolveRequests and send Divide to TM
            //TODO if status comes from CN, send all problems divided for this node

            var node = GetActiveNode(deserializedStatusMessage.Id);

            var nodeTask = GetTaskForNode(node);

            return SerializeMessage(nodeTask);
        }

        private ComputationMessage GetTaskForNode(NodeEntry node)
        {
            switch (node.Type)
            {
                case RegisterType.TaskManager:
                    return GetTaskForTaskManager(node);
                case RegisterType.ComputationalNode:
                    return null;
                default:
                    _logger.Error("GetTaskForNode error: Unknown node type");
                    return null;
            }
        }

        private ComputationMessage GetTaskForTaskManager(NodeEntry node)
        {
            var divideMessage = GetDivideProblemMessageForType(node.Type);
            if (divideMessage != null)
                return divideMessage;
            var partialSolutionsMessage = GetPartialSolutionForType(node.Type);
            if (partialSolutionsMessage != null)
                return partialSolutionsMessage;
            return null;
        }

        private SolutionsMessage GetPartialSolutionForType(RegisterType type)
        {
            var partialSolution = partialSolutions.FirstOrDefault(x => x.ProblemType == type.ToString());
            return partialSolution;
        }

        /// <summary>
        /// Dequeues first solveReqest of given type and returns appropiate DivideProblemMessage for SolveRequest
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private DivideProblemMessage GetDivideProblemMessageForType(RegisterType type)
        {
            DivideProblemMessage divideProblemMessage = null;
            var solveRequest = solveRequests.FirstOrDefault(x => x.Value.ProblemType == type.ToString());
            if (solveRequest.Value != null)
            {
                divideProblemMessage = new DivideProblemMessage()
                {
                    ComputationalNodes = (ulong) activeNodes.Count,
                    Data = new byte[0],
                    ProblemType = type.ToString(),
                    Id = solveRequest.Key
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
                    if (!activeNodes.TryGetValue(statusMessage.Id, out node))
                        return;
                    node.LastStatusSentTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Could not update nodes lifetime. NodeId: " + statusMessage.Id + ". Exception: " + ex.ToString());
            }
        }

        private string ProcessCaseSolutionRequest(string message)
        {
            throw new NotImplementedException();
            var deserializedSolutionRequestMessage = DeserializeMessage<SolutionRequestMessage>(message);
            //solveRequestMessageQueue.Enqueue(deserializedSolutionRequestMessage);
            Solution s = new Solution()
            {
                ComputationsTime = 100,
                Data = new byte[] { 0, 0, 10 },
                TaskId = 23,
                TaskIdSpecified = true,
                TimeoutOccured = true,
                Type = SolutionType.Final
            };
            var solveSolutions = new SolutionsMessage()
            {
                Id = 1,
                ProblemType = "TSP",
                CommonData = new byte[] { 0, 0, 22 },
                Solutions = new Solution[] { s }
            };
            return SerializeMessage<SolutionsMessage>(solveSolutions);
        }

        private string ProcessCaseSolveRequest(string message)
        {
            var deserializedMessage = DeserializeMessage<SolveRequestMessage>(message);
            
            ulong solutionId = GenerateNewSolutionId();

            SolveRequestResponseMessage solveRequestResponse = new SolveRequestResponseMessage() { Id = solutionId };

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