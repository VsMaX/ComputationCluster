using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Communication_Library;

namespace Copmutational_Client
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
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList[0];

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

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

        public void SendSolveRequest(SolveRequestMessage problemRequest)
        {
            try
            {
                // Sending message 
                //<Client Quit> is the sign for end of data 
                string theMessageToSend = "dupa dupa";
                byte[] msg = Encoding.Unicode.GetBytes(theMessageToSend + "<Client Quit>");

                // Sends data to a connected Socket. 
                int bytesSend = senderSock.Send(msg);

                ReceiveDataFromServer();
            }
            catch (Exception exc) {
                throw exc;
            }
        }

        public void ReceiveDataFromServer()
        {
            try
            {
                // Receives data from a bound Socket. 
                int bytesRec = senderSock.Receive(bytes);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

                // Continues to read the data till data isn't available 
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
                }

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
