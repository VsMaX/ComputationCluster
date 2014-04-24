using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using Communication_Library;

namespace Computational_Client
{
    public class ComputationClient : IDisposable
    {
        byte[] bytes = new byte[1024];
        private CommunicationModule communicationModule;
        private Socket socket;

        public ComputationClient(string ip, int port)
        {
            communicationModule = new CommunicationModule(ip, port, 5000);
        }

        public void SendSolveRequest(SolveRequestMessage solveRequestMessage)
        {
            try
            {
                socket = communicationModule.SetupClient();
                communicationModule.Connect(socket);
                var serializer = new ComputationSerializer<SolveRequestMessage>();
                var message = serializer.Serialize(solveRequestMessage);
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                //communicationModule.SendData(byteMessage);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
        }

        public void SendSolutionRequest(SolutionRequestMessage solutionRequestMessage)
        {
            try
            {
                communicationModule.Connect(socket);
                var serializer = new ComputationSerializer<SolutionRequestMessage>();
                var message = serializer.Serialize(solutionRequestMessage);
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                //communicationModule.SendData(byteMessage);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
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
            
        }

        //
        //public ComputationClient()
        //{
            
        //}

        //public void SendProblemRequest(SolveRequestMessage problemRequestMessage)
        //{

        //}

        //public void Connect(string ip)
        //{
            
        //    throw new NotImplementedException();
        //}

        //public void SendSolveRequest(SolveRequestMessage problemRequest)
        //{
        //    throw new NotImplementedException();
        //}
        public void Dispose()
        {
            communicationModule.Dispose();
        }
    }
}
