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
        private ICommunicationModule communicationModule;
        private Socket clientSocket;

        public ComputationClient(string ip, int port)
        {
            communicationModule = new CommunicationModule(ip, port, 50000);
        }

        public void SendSolveRequest(SolveRequestMessage solveRequestMessage)
        {
            try
            {
                clientSocket = communicationModule.SetupClient();
                communicationModule.Connect(clientSocket);
                var serializer = new ComputationSerializer<SolveRequestMessage>();
                var message = serializer.Serialize(solveRequestMessage);
                //byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                
                communicationModule.SendData(message, clientSocket);
                Thread.Sleep(50000);
                communicationModule.CloseSocket(clientSocket);

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
                clientSocket = communicationModule.SetupClient();
                communicationModule.Connect(clientSocket);
                var serializer = new ComputationSerializer<SolutionRequestMessage>();
                var message = serializer.Serialize(solutionRequestMessage);
                //byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                communicationModule.SendData(message, clientSocket);
                Thread.Sleep(50000);
                communicationModule.CloseSocket(clientSocket);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
        }

        public string ReceiveDataFromServer()
        {
            clientSocket = communicationModule.SetupClient();
            communicationModule.Connect(clientSocket);
            var data = communicationModule.ReceiveData(clientSocket);
            Trace.WriteLine("Response: " + data.ToString());
            communicationModule.CloseSocket(clientSocket);
            return data;
        }
        public void Dispose()
        {
            communicationModule.Dispose();
        }

        //public void Disconnect()
        //{
            
        //}

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


        
    }
}
