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

        private Socket socket { get; set; }

        public Socket handler { get; set; }

        private IPEndPoint ipEndPoint { get; set; }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public CommunicationModule(string ip, int port)
        {
            this.port = port;
            this.ip = ip;
        }

        public void SetupListening()
        {
            //do
            //{
                try
                {
                    Trace.WriteLine("");
                    this.SetSocketForListening();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            //}
            //while (this.socket.IsBound);
        }

        private void SetSocketForListening()
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
            this.ipEndPoint = new IPEndPoint(ipAddr, this.port);

            // Create one Socket object to listen the incoming connection 
            this.socket = new Socket(
                ipAddr.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            // Associates a Socket with a local endpoint 
            this.socket.Bind(this.ipEndPoint);
            Trace.WriteLine("Socket set up for listening");
        }

        public void StartListening()
        {
            // Places a Socket in a listening state and specifies the maximum 
            // Length of the pending connections queue 
            this.socket.Listen(100);
            Trace.WriteLine("Socket is now listening on " + this.ipEndPoint.Address + " port: " + this.ipEndPoint.Port);
        }

        public void SetupConnecting()
        {
            if (this.socket != null && this.socket.Connected)
                return;

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

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = IPAddress.Parse(ip);

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                // Create one Socket object to setup Tcp connection 
                this.socket = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                this.socket.NoDelay = false;   // Using the Nagle algorithm 
            }

            catch (Exception exc)
            {
                Trace.WriteLine("Error connecting to server");
                throw exc;
            }
        }

        public void Connect()
        {
            try
            {
                // Establishes a connection to a remote host 
                this.socket.Connect(ipEndPoint);
                Trace.WriteLine("Connected");
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (this.socket == null) return;
                // Disables sends and receives on a Socket. 
                this.socket.Shutdown(SocketShutdown.Both);

                //Closes the Socket connection and releases all resources 
                this.socket.Close();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        public void SendData(string str)
        {
            // Sends data to a connected Socket. 
            int bytesCount = 0;
            byte[] message = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, message, 0, message.Length);

            while (bytesCount != message.Length)
            {
                bytesCount += this.socket.Send(message);
                Trace.WriteLine("Sent " + bytesCount.ToString() + " bytes");
            }
        }

        public string ReceiveData()
        {
            try
            {
                //this.socket.ReceiveTimeout = 3000;
                byte[] buffer = new byte[1024];
                int bytesRec = 0;

                // Converts byte array to string 
                String theMessageToReceive = String.Empty;

                // Read the data till data isn't available 
                //while (this.socket.Available > 0)
                //{
                    bytesRec = this.socket.Receive(buffer);
                    theMessageToReceive += Encoding.UTF8.GetString(buffer, 0, bytesRec);
                    Trace.WriteLine("Received data: " + theMessageToReceive);
                //}
                //Set default value
                //this.socket.ReceiveTimeout = 0;
                return theMessageToReceive;
            }
            catch(SocketException ex)
            {
                //Set default value
                //this.socket.ReceiveTimeout = 0;
                Trace.WriteLine(ex.ToString());
                return String.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw ex;
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

        ///////////////////////////////////////////////////////////////////////////////////////

        //public void SendDataAsync(byte[] message)
        //{
        //    // Sends data to a connected Socket. 
        //    int bytesCount = 0;
        //    while (bytesCount != message.Length)
        //    {
        //        bytesCount += this.socket.Send(message);
        //        Trace.WriteLine("Sent " + bytesCount.ToString() + " bytes to server");
        //    }
        //}

        //public string ReceiveDataAsync()
        //{
        //    //while (true)
        //    //{
        //        // Set the event to nonsignaled state.
        //        //allDone.Reset();

        //        // Start an asynchronous socket to listen for connections.
        //        Console.WriteLine("Waiting for a connection...");
        //        this.socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this.socket);
        //        //allDone.WaitOne();
        //        // Wait until a connection is made before continuing.
        //    //}

        //    return String.Empty;
        //}

        ////public void Dispose()
        ////{
        ////    Disconnect();
        ////}

        //public void AcceptCallback(IAsyncResult ar)
        //{
        //    Trace.WriteLine("Accepted callback");
        //    allDone.Set();
        //    Thread.Sleep(2000);

        //    // A new Socket to handle remote host communication 
        //    try
        //    {
        //        this.HandleCommunication(ar);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex.ToString());
        //    }
        //}

        //public void HandleCommunication(IAsyncResult ar)
        //{
        //    Socket listener = null;

        //    // Receiving byte array 
        //    byte[] buffer = new byte[1024];

        //    // Get Listening Socket object 
        //    listener = (Socket)ar.AsyncState;

        //    // Create a new socket 
        //    this.handler = listener.EndAccept(ar);

        //    // Using the Nagle algorithm 
        //    this.handler.NoDelay = false;

        //    // Creates one object array for passing data 
        //    object[] obj = new object[2];
        //    obj[0] = buffer;
        //    obj[1] = this.handler;

        //    // Begins to asynchronously receive data 
        //    this.handler.BeginReceive(
        //        buffer,        // An array of type Byt for received data
        //        0,             // The zero-based position in the buffer
        //        buffer.Length, // The number of bytes to receive
        //        SocketFlags.None,// Specifies send and receive behaviors
        //        new AsyncCallback(this.ReceiveCallback),//An AsyncCallback delegate
        //        obj            // Specifies infomation for receive operation
        //        );

        //    // Begins an asynchronous operation to accept an attempt 
        //    AsyncCallback aCallback = new AsyncCallback(this.AcceptCallback);
        //    listener.BeginAccept(aCallback, listener);
        //}

        //public void ReceiveCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        string content = this.ReadMessage(ar);
        //        Trace.WriteLine("Received message: \n" + content);

        //        //var response = this.ProcessMessage(content, this.GetMessageName(content));
        //        var response = String.Empty;
        //        byte[] bytes = CommunicationModule.ConvertStringToData(response);
        //        handler.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), handler);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex.ToString());
        //    }
        //}

        //public string ReadMessage(IAsyncResult ar)
        //{
        //    // Fetch a user-defined object that contains information 
        //    object[] obj = new object[2];
        //    obj = (object[])ar.AsyncState;

        //    // Received byte array 
        //    byte[] buffer = (byte[])obj[0];

        //    // A Socket to handle remote host communication. 
        //    handler = (Socket)obj[1];

        //    // Received message 
        //    string content = string.Empty;

        //    // The number of bytes received. 
        //    int bytesRead = handler.EndReceive(ar);

        //    return CommunicationModule.ConvertDataToString(buffer, bytesRead);
        //}

        //private static void SendCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the socket from the state object.
        //        Socket handler = (Socket)ar.AsyncState;

        //        // Complete sending the data to the remote device.
        //        int bytesSent = handler.EndSend(ar);
        //        Console.WriteLine("Sent {0} bytes back.", bytesSent);

        //        handler.Shutdown(SocketShutdown.Both);
        //        handler.Close();

        //    }
        //    catch (Exception e)
        //    {
        //        Trace.WriteLine(e.ToString());
        //    }
        //}

        //private string GetMessageName(string message)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    try
        //    {
        //        doc.LoadXml(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("Error parsing xml document: " + message + "exception: " + ex.ToString());
        //        return String.Empty;

        //        //TODO logowanie
        //    }
        //    XmlElement root = doc.DocumentElement;
        //    return root.Name;
        //}

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
