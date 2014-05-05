using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using Communication_Library;
using System.Windows.Controls;

namespace Computational_Client
{
    public class ComputationClient : IDisposable
    {
        byte[] bytes = new byte[1024];
        private ICommunicationModule communicationModule;
        private Socket clientSocket;

        public ComputationClient(string ip, int port, int receiveTimeout)
        {
            communicationModule = new CommunicationModule(ip, port, receiveTimeout);
        }

        public string SendSolveRequest(SolveRequestMessage solveRequestMessage)
        {
            try
            {
                clientSocket = communicationModule.SetupClient();
                communicationModule.Connect(clientSocket);

                var serializer = new ComputationSerializer<SolveRequestMessage>();
                var message = serializer.Serialize(solveRequestMessage);
                
                communicationModule.SendData(message, clientSocket);

                var response = communicationModule.ReceiveData(clientSocket);
                communicationModule.CloseSocket(clientSocket);

                return response;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }

            return String.Empty;
        }

        public string SendSolutionRequest(SolutionRequestMessage solutionRequestMessage)
        {
            try
            {
                clientSocket = communicationModule.SetupClient();
                communicationModule.Connect(clientSocket);

                var serializer = new ComputationSerializer<SolutionRequestMessage>();
                var message = serializer.Serialize(solutionRequestMessage);

                communicationModule.SendData(message, clientSocket);

                var response = communicationModule.ReceiveData(clientSocket);
                communicationModule.CloseSocket(clientSocket);

                return response;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
            return String.Empty;
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
