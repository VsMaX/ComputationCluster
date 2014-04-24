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

namespace Communication_Library
{
    public class CommunicationModule : BaseNode, IDisposable, ICommunicationModule
    {
        private string ip;
        private int port;
        public Socket handler { get; set; }
        public readonly int ReadTimeoutMs;

        private IPEndPoint ipEndPoint { get; set; }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public CommunicationModule(string ip, int port, int readTimeoutMs)
        {
            this.port = port;
            this.ip = ip;
            this.ReadTimeoutMs = readTimeoutMs;
        }

        public Socket SetupServer()
        {
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
            ipEndPoint = new IPEndPoint(ipAddr, this.port);

            // Create one Socket object to listen the incoming connection 
            var socket = new Socket(
                ipAddr.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // Associates a Socket with a local endpoint 
            socket.Bind(this.ipEndPoint);
            // Places a Socket in a listening state and specifies the maximum 
            // Length of the pending connections queue 
            socket.Listen(100);
            Trace.WriteLine("Socket is now listening on " + this.ipEndPoint.Address + " port: " + this.ipEndPoint.Port);

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

            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = IPAddress.Parse(ip);

            // Creates a network endpoint 
            ipEndPoint = new IPEndPoint(ipAddr, port);

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
            if (socket != null && !socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }

            socket.Close();
            //Closes the Socket connection and releases all resources 
        }

        public void SendData(string str, Socket socket)
        {
            // Sends data to a connected Socket. 
            int bytesCount = 0;
            byte[] message = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, message, 0, message.Length);

            while (bytesCount != message.Length)
            {
                bytesCount += socket.Send(message);
                Trace.WriteLine("Sent " + bytesCount.ToString() + " bytes");
            }
        }

        public string ReceiveData(Socket socket)
        {
            //this.socket.ReceiveTimeout = 3000;
            byte[] buffer = new byte[StateObject.BufferSize];

            // Converts byte array to string 
            var state = new StateObject();

            var result = socket.BeginReceive(buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            result.AsyncWaitHandle.WaitOne(ReadTimeoutMs);

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
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Trace.WriteLine(String.Format("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content));
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        public static string ConvertDataToString(byte[] buffer, int bytesRead)
        {
            if (bytesRead > 0)
                return Encoding.UTF8.GetString(buffer, 0,
                        bytesRead);
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
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
    }
}
