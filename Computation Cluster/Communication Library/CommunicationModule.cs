using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using log4net.Repository.Hierarchy;

namespace Communication_Library
{
    [MethodBoundary]
    public class CommunicationModule : IDisposable, ICommunicationModule
    {
        private string ip;
        private int port;
        public readonly int ReadTimeoutMs;

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static readonly ILog _logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CommunicationModule(string ip, int port, int readTimeoutMs)
        {
            this.port = port;
            this.ip = ip;
            this.ReadTimeoutMs = readTimeoutMs;
        }

        public Socket SetupServer()
        {
            Socket socket = null;
            // Creates one SocketPermission object for access restrictions
            var permission = new SocketPermission(
                NetworkAccess.Accept, // Allowed to accept connections 
                TransportType.Tcp, // Defines transport types 
                this.ip, // The IP addresses of local host 
                SocketPermission.AllPorts // Specifies all ports 
                );

            // Ensures the code to have permission to access a Socket 
            permission.Demand();

            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = IPAddress.Parse(this.ip);

            // Creates a network endpoint 
            var ipEndPoint = new IPEndPoint(ipAddr, this.port);

            // Create one Socket object to listen the incoming connection 
            socket = new Socket(
                ipAddr.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // Associates a Socket with a local endpoint 
            socket.Bind(ipEndPoint);
            // Places a Socket in a listening state and specifies the maximum 
            // Length of the pending connections queue 
            socket.Listen(100);

            return socket;
        }

        public Socket SetupClient()
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

            IPAddress ipAddr = IPAddress.Parse(ip);

            // Create one Socket object to setup Tcp connection 
            var socket = new Socket(
                ipAddr.AddressFamily,// Specifies the addressing scheme 
                SocketType.Stream,   // The type of socket  
                ProtocolType.Tcp     // Specifies the protocols  
                );

            socket.NoDelay = false;   // Using the Nagle algorithm 
            return socket;
        }

        public void Connect(Socket socket)
        {
            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = IPAddress.Parse(ip);
            var ipEndPoint = new IPEndPoint(ipAddr, port);
            // Establishes a connection to a remote host 
            socket.Connect(ipEndPoint);
        }

        public Socket Accept(Socket socket)
        {
            return socket.Accept();
        }

        public void CloseSocket(Socket socket)
        {
            // Disables sends and receives on a Socket. 
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            else
            {
                _logger.Error("Could not close socket");
            }

            //Closes the Socket connection and releases all resources 
        }

        public void SendData(string str, Socket socket)
        {
            // Sends data to a connected Socket. 
            int bytesCount = 0;
            byte[] message = ConvertStringToData(str);

            _logger.Debug("Sending message: " + str);

            while (bytesCount != message.Length)
            {
                bytesCount += socket.Send(message);
                _logger.Debug("Sent " + bytesCount + " bytes");
            }
        }

        public string ReceiveData(Socket socket)
        {
            // Converts byte array to string 
            var state = new StateObject();

            state.workSocket = socket;

            receiveDone.Reset();

            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            receiveDone.WaitOne(ReadTimeoutMs);

            _logger.Debug("Ended receiving data");

            return state.sb.ToString();
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            
            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                _logger.Debug(String.Format("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content));
                receiveDone.Set();
            }
        }

        public static string ConvertDataToString(byte[] buffer, int bytesRead)
        {
            if (bytesRead > 0)
            {
                var message = Encoding.UTF8.GetString(buffer, 0,
                        bytesRead);
                //message = message.Replace("\0", String.Empty);
                //message = message.Trim();
                return message;
            }
            return String.Empty;
        }

        public static byte[] ConvertStringToData(string response)
        {
            return Encoding.UTF8.GetBytes(response);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1000 * 1024;
            // Receive buffer.
            public byte[] buffer = new byte[1000 * 1024];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
    }
}
