using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Communication_Library;

namespace Computational_Client
{
    public class ComputationClient
    {
        Socket senderSock;
        byte[] bytes = new byte[1024];

        public void Connect(string ip)
        {
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    "",                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                //IPHostEntry ipHost = Dns.GetHostEntry("192.168.110.34");

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = IPAddress.Parse("192.168.0.100");

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 22222);

                // Create one Socket object to setup Tcp connection 
                senderSock = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                senderSock.NoDelay = false;   // Using the Nagle algorithm 

                // Establishes a connection to a remote host 
                senderSock.Connect(ipEndPoint);
                //tbStatus.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();
            }
            catch (Exception exc) {
                throw exc;
            }

        }

        public void SendSolveRequest(SolveRequest msgg)
        {
            try
            {
                // Sending message 
                //<Client Quit> is the sign for end of data 
                var ser = new TestSerializeDeserialize();
                string toSend = ser.Serialize(msgg);

                byte[] theMessageToSend = Encoding.UTF8.GetBytes(toSend);
                //byte[] msg = Encoding.UTF8.GetBytes(theMessageToSend.ToString() + "<Client Quit>");
                // Sends data to a connected Socket. 
                int bytesSend = senderSock.Send(theMessageToSend);

                //ReceiveDataFromServer();
            }
            catch (Exception exc) {
               throw exc;
            }
        }

        public SolveRequest ReceiveDataFromServer()
        {
            try
            {
                var ser = new TestSerializeDeserialize();

                // Receives data from a bound Socket. 
                int bytesRec = senderSock.Receive(bytes);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                // Continues to read the data till data isn't available 
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    theMessageToReceive += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                }

                SolveRequest deserializedObject = ser.Deserialize(theMessageToReceive);
                return deserializedObject;
            }
            catch (Exception exc) {
                throw exc; 
            }
        }

        public void Disconnect()
        {
            try
            {
                // Disables sends and receives on a Socket. 
                senderSock.Shutdown(SocketShutdown.Both);

                //Closes the Socket connection and releases all resources 
                senderSock.Close();

            }
            catch (Exception exc) {
                throw exc;
            }
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
    }
}
