using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;

namespace Computational_Node
{
    public class ComputationnalNode : BaseNode
    {
        private int serverPort;
        private string serverIp;
        private CommunicationModule communicationModule;

        public ulong NodeId { get; set; }
        public TimeSpan Timeout { get; set; }

        public ComputationnalNode(string serverIp, int serverPort)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort);
            RegisterAtServer();
        }

        public void RegisterAtServer()
        {
            var registerMessage = new RegisterMessage()
            {
                ParallelThreads = 8,
                SolvableProblems = new string[] {"TSP"},
                Type = RegisterType.ComputationalNode
            };
            var messageString = SerializeMessage(registerMessage);
            var messageBytes = CommunicationModule.ConvertStringToData(messageString);
            communicationModule.Connect();
            communicationModule.SendData(messageBytes);
            var response = communicationModule.ReceiveData();
            Trace.WriteLine("Response: " + response.ToString());
            var deserializedResponse = DeserializeMessage<RegisterResponseMessage>(response);
            Trace.WriteLine("Response has been deserialized");
            NodeId = deserializedResponse.Id;
            Timeout = deserializedResponse.Time;
        }

        public void SendStatus()
        {
            communicationModule.Connect();
            var testStatusThread = new StatusThread() {HowLong = 100, TaskId = 1, State = StatusThreadState.Busy, ProblemType = "TSP", ProblemInstanceId = 1, ProblemInstanceIdSpecified = true, TaskIdSpecified = true};
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
        }

        public void Disconnect()
        {
            communicationModule.Disconnect();
        }
    }
}
