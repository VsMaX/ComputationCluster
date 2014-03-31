using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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
                var serializer = new ComputationSerializer<SolveRequestMessage>();
                var message = serializer.Serialize(solveRequestMessage);
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                communicationModule.SendData(byteMessage);
                var response = communicationModule.ReceiveData();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                //TODO logowanie
            }
        }

        public SolveRequestMessage ReceiveDataFromServer()
        {
            //try
            //{
            //    var ser = new TestSerializeDeserialize();

            //    // Receives data from a bound Socket. 
            //    int bytesRec = senderSock.Receive(bytes);

            //    // Converts byte array to string 
            //    String theMessageToReceive = Encoding.UTF8.GetString(bytes, 0, bytesRec);

            //    // Continues to read the data till data isn't available 
            //    while (senderSock.Available > 0)
            //    {
            //        bytesRec = senderSock.Receive(bytes);
            //        theMessageToReceive += Encoding.UTF8.GetString(bytes, 0, bytesRec);
            //    }

            //    SolveRequest deserializedObject = ser.Deserialize(theMessageToReceive);
            //    return deserializedObject;
            //}
            //catch (Exception exc) {
            //    throw exc; 
            //}
            throw new NotImplementedException();
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
