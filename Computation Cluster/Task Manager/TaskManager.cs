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

        public TaskManager(string serverIp, int serverPort)
        {
            communicationModule = new CommunicationModule(serverIp, serverPort);
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
            Trace.WriteLine("Response has been deserialized");
        }

        public int NodeId { get; set; }
    }

}
