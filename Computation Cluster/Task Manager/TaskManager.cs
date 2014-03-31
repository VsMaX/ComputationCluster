using System;
using System.Collections.Generic;
using System.Linq;
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

        public ulong NodeId { get; set; }
        public TimeSpan Timeout { get; set; }

        public TaskManager(string serverIp, int serverPort)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort);
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
            communicationModule.Connect();
            communicationModule.SendData(messageBytes);
            var response = communicationModule.ReceiveData();
            Trace.WriteLine("Response: " + response.ToString());
            var deserializedResponse = DeserializeMessage<RegisterResponseMessage>(response);
            this.NodeId = deserializedResponse.Id;
            this.Timeout = deserializedResponse.Time;
            Trace.WriteLine("Response has been deserialized");
            communicationModule.Disconnect();
        }

        public void SendStatus()
        {
            communicationModule.Connect();
            var testStatusThread = new StatusThread() { HowLong = 100, TaskId = 1, State = StatusThreadState.Busy, ProblemType = "TSP", ProblemInstanceId = 1, ProblemInstanceIdSpecified = true, TaskIdSpecified = true };
            var statusMessage = new StatusMessage()
            {
                Id = NodeId,
                Threads = new StatusThread[] { testStatusThread }
            };
            var statusMessageString = SerializeMessage(statusMessage);
            var statusMessageBytes = CommunicationModule.ConvertStringToData(statusMessageString);
            communicationModule.Connect();
            communicationModule.SendData(statusMessageBytes);
            var statusMessageResponse = communicationModule.ReceiveData();
            Trace.WriteLine("status response: " + statusMessageResponse);
            communicationModule.Disconnect();
        }

        public string ReceiveDataFromServer()
        {
            communicationModule.Connect();
            var data = communicationModule.ReceiveData();
            Trace.WriteLine("Response: " + data.ToString());
            return data;
        }

        public void Disconnect()
        {
            communicationModule.Disconnect();
        }

        public void DivideProblem()
        {
            communicationModule.Connect();
            SolvePartialProblemsPartialProblem sp = new SolvePartialProblemsPartialProblem() { Data = new byte[] { 1, 2, 3 }, TaskId = 4 };
            var partialproblems = new PartialProblemsMessage() { Id = 2, CommonData = new byte[] { 1, 2, 3 }, PartialProblems = new SolvePartialProblemsPartialProblem[] { sp }, ProblemType = "TSP", SolvingTimeout = 30, SolvingTimeoutSpecified = true };
            var msg = SerializeMessage<PartialProblemsMessage>(partialproblems);
            var msgBytes = CommunicationModule.ConvertStringToData(msg);
            communicationModule.SendData(msgBytes);
            communicationModule.Disconnect();
        }
    }
}
