using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication_Library
{
    public class CommunicationModule : IDisposable
    {
        Socket senderSock;
        private string _ip;
        private int _port;

        public CommunicationModule(string ip, int port)
        {
            _port = port;
            _ip = ip;
        }

        public void Connect(string ip, int port)
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
                IPAddress ipAddr = IPAddress.Parse(ip);

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

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
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public int SendData(byte[] message)
        {
            Connect(_ip, _port);
            // Sends data to a connected Socket. 
            int bytesSent = senderSock.Send(message);
            Disconnect();
            return bytesSent;
        }

        public void Disconnect()
        {
            // Disables sends and receives on a Socket. 
            senderSock.Shutdown(SocketShutdown.Both);

            //Closes the Socket connection and releases all resources 
            senderSock.Close();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public static string ConvertDataToString(byte[] buffer, int bytesRead)
        {
            if(bytesRead > 0)
                return Encoding.UTF8.GetString(buffer, 0,
                        bytesRead);
            return String.Empty;
        }

        public static byte[] ConvertStringToData(string response)
        {
            return Encoding.UTF8.GetBytes(response);
        }

        public string ReceiveData()
        {
            try
            {
                // Receives data from a bound Socket. 
                byte[] buffer = new byte[1024];
                int bytesRec = senderSock.Receive(buffer);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.UTF8.GetString(buffer, 0, bytesRec);

                // Continues to read the data till data isn't available 
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(buffer);
                    theMessageToReceive += Encoding.UTF8.GetString(buffer, 0, bytesRec);
                }

                return theMessageToReceive;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
