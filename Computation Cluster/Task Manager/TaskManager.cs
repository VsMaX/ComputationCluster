using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Communication_Library;

namespace Task_Manager
{
    public class TaskManager : BaseNode
    {
        private int serverPort;
        private string serverIp;
        private CommunicationModule communicationModule;
        public TSP tsp;
        public ulong NodeId { get; set; }
        public TimeSpan Timeout { get; set; }
        private Socket socket;

        public TaskManager(string serverIp, int serverPort)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort, Timeout.Milliseconds);
            RegisterAtServer();
        }

        public void RegisterAtServer()
        {
            var registerMessage = new RegisterMessage()
            {
                ParallelThreads = 8,
                SolvableProblems = new string[] { "TSP" },
                Type = RegisterType.TaskManager
            };
            var messageString = SerializeMessage(registerMessage);
            var messageBytes = CommunicationModule.ConvertStringToData(messageString);
            socket = communicationModule.SetupClient();
            communicationModule.Connect(socket);
            //communicationModule.SendData(messageBytes);
            var response = communicationModule.ReceiveData(socket);
            Trace.WriteLine("Response: " + response.ToString());
            var deserializedResponse = DeserializeMessage<RegisterResponseMessage>(response);
            this.NodeId = deserializedResponse.Id;
            this.Timeout = deserializedResponse.TimeoutTimeSpan;
            Trace.WriteLine("Response has been deserialized");
            //communicationModule.Disconnect();
        }

        public void SendStatus()
        {
            communicationModule.Connect(socket);
            var testStatusThread = new StatusThread() { HowLong = 100, TaskId = 1, State = StatusThreadState.Busy, ProblemType = "TSP", ProblemInstanceId = 1, TaskIdSpecified = true };
            var statusMessage = new StatusMessage()
            {
                Id = NodeId,
                Threads = new StatusThread[] { testStatusThread }
            };
            var statusMessageString = SerializeMessage(statusMessage);
            var statusMessageBytes = CommunicationModule.ConvertStringToData(statusMessageString);
            communicationModule.Connect(socket);
            //communicationModule.SendData(statusMessageBytes);
            var statusMessageResponse = communicationModule.ReceiveData(socket);
            if (statusMessageResponse.Length > 0)
                DivideProblem(statusMessageResponse);
         
            Trace.WriteLine("status response: " + statusMessageResponse);
            ReceiveDataFromServer();
            //communicationModule.Disconnect();
        }

        public string ReceiveDataFromServer()
        {
            communicationModule.Connect(socket);
            var data = communicationModule.ReceiveData(socket);
            
            Trace.WriteLine("Response: " + data.ToString());
            
            return data;
        }

        public void Disconnect()
        {
            //communicationModule.Disconnect();
        }

        public void DivideProblem(string statusMessageResponse)
        {
            var serializer = new ComputationSerializer<DivideProblemMessage>();
            DivideProblemMessage dpm = serializer.Deserialize(statusMessageResponse);

            tsp = new TSP(dpm.Data);
            tsp.DivideProblem((int)dpm.ComputationalNodes);
            SolvePartialProblemsPartialProblem[] solvepp = new SolvePartialProblemsPartialProblem[tsp.PartialProblems.Length];


            for (int i = 0; i < tsp.PartialProblems.Length; i++)
            {
                solvepp[i] = new SolvePartialProblemsPartialProblem() { Data = tsp.PartialProblems[i], TaskId = (ulong)i };    
            }

                communicationModule.Connect(socket);
           
            //SolvePartialProblemsPartialProblem sp = new SolvePartialProblemsPartialProblem() { Data = new byte[] { 1, 2, 3 }, TaskId = 4 };
            var partialproblems = new PartialProblemsMessage() { Id = dpm.Id, CommonData = dpm.Data, PartialProblems = solvepp, ProblemType = tsp.Name, SolvingTimeout = 30, SolvingTimeoutSpecified = true };
            //var msg = SerializeMessage<PartialProblemsMessage>(partialproblems);
            //var msgBytes = CommunicationModule.ConvertStringToData(msg);
            //communicationModule.SendData(msgBytes);

            //communicationModule.Disconnect();
        }
    }
}
