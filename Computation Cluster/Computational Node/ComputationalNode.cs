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

namespace Computational_Node
{
    public class ComputationnalNode : BaseNode
    {
        private Thread listeningThread;
        private Thread processingThread;
        private CancellationTokenSource listeningThreadCancellationTokenSource;
        private CancellationTokenSource processingThreadCancellationToken;

        private int serverPort;
        private string serverIp;
        private CommunicationModule communicationModule;

        public ulong NodeId { get; set; }
        public TimeSpan Timeout { get; set; }
        private Socket socket;

        public ComputationnalNode(string serverIp, int serverPort, int timeout)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort, timeout);
            RegisterAtServer();
        }

        public void RegisterAtServer()
        {

            this.StartListeningThread();

            socket = communicationModule.SetupClient();
            communicationModule.Connect(socket);

            this.Register();
            this.SendStatus();

            //var registerMessage = new RegisterMessage()
            //{
            //    ParallelThreads = 8,
            //    SolvableProblems = new string[] { "TSP" },
            //    Type = RegisterType.ComputationalNode
            //};
            //var messageString = SerializeMessage(registerMessage);
            ////var messageBytes = CommunicationModule.ConvertStringToData(messageString);
            //communicationModule.SendData(messageString, socket);

            //var response = communicationModule.ReceiveData(socket);
            //Trace.WriteLine("Response: " + response.ToString());
            //var deserializedResponse = DeserializeMessage<RegisterResponseMessage>(response);
            //Trace.WriteLine("Response has been deserialized");
            //NodeId = deserializedResponse.Id;
            //Timeout = deserializedResponse.TimeoutTimeSpan;

            //var statusMessage = new StatusMessage() { };
            //messageString = this.SerializeMessage(statusMessage);
            //this.communicationModule.SendData(messageString,socket);

            communicationModule.CloseSocket(socket);
            //this.StartProcessingThread();
        }

        private void Register()
        {
            var registerMessage = new RegisterMessage()
            {
                ParallelThreads = 8,
                SolvableProblems = new string[] { "TSP" },
                Type = RegisterType.ComputationalNode
            };
            var messageString = SerializeMessage(registerMessage);
            this.communicationModule.SendData(messageString, socket);
        }

        public void SendStatus()
        {
            var statusMessage = new StatusMessage() { };
            var messageString = this.SerializeMessage(statusMessage);
            this.communicationModule.SendData(messageString, socket);
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

        private void ListeningThread(object obj)
        {
            this.socket = this.communicationModule.SetupClient();
            this.communicationModule.Connect(socket);
            while (!listeningThreadCancellationTokenSource.IsCancellationRequested)
            {
                //var clientSocket = communicationModule.Accept(socket);
                //Trace.WriteLine("Accepted connection");

                var receivedMessage = communicationModule.ReceiveData(socket);
                Trace.WriteLine("Received data: " + receivedMessage);

                string result = String.Empty;
                if (!String.IsNullOrEmpty(receivedMessage))
                    result = ProcessMessage(receivedMessage);
                Trace.WriteLine("Message processed, response: " + result);

                if (!String.IsNullOrEmpty(result))
                    communicationModule.SendData(result, socket);
                Trace.WriteLine("Reponse sent ");

                communicationModule.CloseSocket(socket);
                Trace.WriteLine("Socket closed");
            }
        }

        private string ProcessMessage(string message)
        {
            var messageName = this.GetMessageName(message);
            Trace.WriteLine("Received " + messageName);
            switch (messageName)
            {
                case "RegisterResponse":
                    return this.ProcessRegisterResponse(message);

                case "DivideProblem":
                    return this.ProcessDivideProblem(message);

                default:
                    Trace.WriteLine("Received another status");
                    Trace.WriteLine("XML Data: " + message);
                    break;
            }
            return String.Empty;
        }

        private string ProcessDivideProblem(string message)
        {
            throw new NotImplementedException();
        }

        private string ProcessRegisterResponse(string message)
        {
            var response = communicationModule.ReceiveData(socket);
            Trace.WriteLine("Response: " + response.ToString());
            return response.ToString();
        }

        private void ProcessingThread(object obj)
        {

        }

        //public void SendStatus()
        //{
        //    //communicationModule.Connect(socket);
        //    //var testStatusThread = new StatusThread() {HowLong = 100, TaskId = 1, State = StatusThreadState.Busy, ProblemType = "TSP", ProblemInstanceId = 1, TaskIdSpecified = true};
        //    //var statusMessage = new StatusMessage()
        //    //{
        //    //    Id = NodeId,
        //    //    Threads = new StatusThread[] { testStatusThread }
        //    //};
        //    //var statusMessageString = SerializeMessage(statusMessage);
        //    //var statusMessageBytes = CommunicationModule.ConvertStringToData(statusMessageString);
        //    //communicationModule.Connect(socket);
        //    ////communicationModule.SendData(statusMessageBytes);
        //    //var statusMessageResponse = communicationModule.ReceiveData(socket);
        //    //Trace.WriteLine("status response: " + statusMessageResponse);
        //}
    }
}
