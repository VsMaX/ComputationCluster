using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using SuperSocket.SocketEngine.Configuration;

namespace Computational_Server
{
    public class ComputationServer : BaseNode
    {
        private Thread listeningThread;
        private Thread processingThread;
        private CancellationTokenSource listeningThreadCancellationTokenSource;
        private CancellationTokenSource processingThreadCancellationToken;
        
        private ConcurrentQueue<SolveRequestMessage> solveRequestMessageQueue;
        public ConcurrentDictionary<ulong, NodeEntry> ActiveNodes { get; set; }
        Socket handler;
        private ICommunicationModule communicationModule;

        public readonly TimeSpan DefaultTimeout;
        private ulong nodesId;
        private object nodesIdLock = new object();
        private Socket serverSocket;

        public ComputationServer(TimeSpan nodeTimeout, ICommunicationModule communicationModule)
        {
            solveRequestMessageQueue = new ConcurrentQueue<SolveRequestMessage>();
            ActiveNodes = new ConcurrentDictionary<ulong, NodeEntry>();
            DefaultTimeout = nodeTimeout;
            nodesId = 0;
            this.communicationModule = communicationModule;
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
            
        }

        public void ListeningThread()
        {
            serverSocket = communicationModule.SetupServer();
            while (!listeningThreadCancellationTokenSource.IsCancellationRequested)
            {
                var clientSocket = communicationModule.Accept(serverSocket);
                Trace.WriteLine("Accepted connection");

                var receivedMessage = communicationModule.ReceiveData(clientSocket);
                Trace.WriteLine("Received data: " + receivedMessage);

                string result = String.Empty;
                if (!String.IsNullOrEmpty(receivedMessage))
                    result = ProcessMessage(receivedMessage);
                Trace.WriteLine("Message processed, response: " + result);

                if(!String.IsNullOrEmpty(result))
                    communicationModule.SendData(result, clientSocket);
                Trace.WriteLine("Reponse sent ");

                communicationModule.CloseSocket(clientSocket);
                Trace.WriteLine("Socket closed");
            }
        }

        private string ProcessMessage(string message)
        {
            var messageName = this.GetMessageName(message);
            Trace.WriteLine("Received " + messageName);
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
                    Trace.WriteLine("Received another status");
                    Trace.WriteLine("XML Data: " + message);
                    break;
            }
            return String.Empty;
        }

        private string ProcessCaseRegister(string message)
        {
            var registerMessage = DeserializeMessage<RegisterMessage>(message);

            var newId = GenerateNewNodeId();
            
            if (RegisterNode(newId, registerMessage.Type, registerMessage.SolvableProblems.ToList(),
                    registerMessage.ParallelThreads) < 0)
                LogError("");

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
        private int RegisterNode(ulong newId, RegisterType type, List<string> solvableProblems, byte parallelThreads)
        {
            var node = new NodeEntry(newId, type, solvableProblems, parallelThreads);
            if (ActiveNodes.TryAdd(newId, node))
                return -1;
            return 0;
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
            var deserializedStatusMessage = DeserializeMessage<StatusMessage>(message);

            //TODO odswiezyc czas zycia CNa na liscie

            //if (IfTaskManager(deserializedStatusMessage.Id))
            //{
            //    var dp = new DivideProblemMessage() { Id = deserializedStatusMessage.Id, ComputationalNodes = 20, Data = new byte[] { 0, 0, 10 }, ProblemType = "TSP" };
            //    return SerializeMessage<DivideProblemMessage>(dp);
            //}
            return string.Empty;
        }

        private string ProcessCaseSolutionRequest(string message)
        {
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

            //TO DO Oczekiwanie na wlasciwego TM i przesłanie do niego problemu
            solveRequestMessageQueue.Enqueue(deserializedMessage);
            var solveRequestResponse = new SolveRequestResponseMessage() { Id = 1 };
            return SerializeMessage<SolveRequestResponseMessage>(solveRequestResponse);
        }

        

        private string GetMessageName(string message)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error parsing xml document: " + message + "exception: " + ex.ToString());
                return String.Empty;

                //TODO logowanie
            }
            XmlElement root = doc.DocumentElement;
            return root.Name;
        }

        public void StopServer()
        {
            listeningThreadCancellationTokenSource.Cancel();
            listeningThread.Join();
            Trace.WriteLine("Stopped listening");
        }
    }
}