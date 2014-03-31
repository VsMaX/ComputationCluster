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

        public ComputationClient(string ip, int port)
        {
            communicationModule = new CommunicationModule(ip, port);
        }

        public void SendSolveRequest(SolveRequestMessage solveRequestMessage)
        {
            try
            {
                communicationModule.Connect();
                var serializer = new ComputationSerializer<SolveRequestMessage>();
                var message = serializer.Serialize(solveRequestMessage);
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                communicationModule.SendData(byteMessage);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
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
