using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication_Library;

namespace Computational_Server
{
    public class ComputationServer
    {
        private int port;
        private string serverIPAddress;

        private object solveRequestMessagesLock = new object();
        private int openConnectionsCount;

        private Queue<SolveRequestMessage> solveRequestMessageQueue;
        public List<NodeEntry> ActiveNodes { get; set; }
        Socket handler;

        public ComputationServer(int _port, string _ipAddress)
        {
            solveRequestMessageQueue = new Queue<SolveRequestMessage>();
            serverIPAddress = _ipAddress;
            port = _port;
            openConnectionsCount = 0;
        }

        public void StartListening()
        {
            //TODO przepisac ladnie i wydzielic mniejsze funkcje
            Socket sListener = null;
            IPEndPoint ipEndPoint = null;
            try
            {
                Trace.WriteLine("");

                // Creates one SocketPermission object for access restrictions
                var permission = new SocketPermission(
                    NetworkAccess.Accept, // Allowed to accept connections 
                    TransportType.Tcp, // Defines transport types 
                    serverIPAddress, // The IP addresses of local host 
                    SocketPermission.AllPorts // Specifies all ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance 
                //IPHostEntry ipHost = Dns.GetHostEntry("192.168.110.38");

                //byte[] ip_byte = new byte[ip_string.Length * sizeof(char)];
                //System.Buffer.BlockCopy(ip_string.ToCharArray(), 0, ip_byte, 0, ip_byte.Length);
                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = IPAddress.Parse(serverIPAddress);

                // Creates a network endpoint 
                ipEndPoint = new IPEndPoint(ipAddr, port);

                // Create one Socket object to listen the incoming connection 
                sListener = new Socket(
                    ipAddr.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );

                // Associates a Socket with a local endpoint 
                sListener.Bind(ipEndPoint);
                Trace.WriteLine("Server listening");
            }
            catch (Exception ex) { Trace.WriteLine(ex.ToString()); }

            try
            {
                // Places a Socket in a listening state and specifies the maximum 
                // Length of the pending connections queue 
                sListener.Listen(100);

                // Begins an asynchronous operation to accept an attempt 
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                sListener.BeginAccept(aCallback, sListener);

                Trace.WriteLine("Server is now listening on " + ipEndPoint.Address + " port: " + ipEndPoint.Port);
            }
            catch (Exception ex) { Trace.WriteLine(ex.ToString()); }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            //TODO przepisac ladnie i wydzielic mniejsze funkcje
            //TODO tutaj powinna sie znajdowac logika odpowiadajaca za rozpoznawanie wiadomosci przychodzacych
            //TODO kazda wiadomosc powinna miec swoja funkcje ktora ja obsluguje
            Trace.WriteLine("Accepted callback");
            Socket listener = null;

            // A new Socket to handle remote host communication 
            try
            {
                // Receiving byte array 
                byte[] buffer = new byte[1024];
                // Get Listening Socket object 
                listener = (Socket)ar.AsyncState;
                // Create a new socket 
                handler = listener.EndAccept(ar);

                // Using the Nagle algorithm 
                handler.NoDelay = false;

                // Creates one object array for passing data 
                object[] obj = new object[2];
                obj[0] = buffer;
                obj[1] = handler;

                // Begins to asynchronously receive data 
                handler.BeginReceive(
                    buffer,        // An array of type Byt for received data 
                    0,             // The zero-based position in the buffer  
                    buffer.Length, // The number of bytes to receive 
                    SocketFlags.None,// Specifies send and receive behaviors 
                    new AsyncCallback(ReceiveCallback),//An AsyncCallback delegate 
                    obj            // Specifies infomation for receive operation 
                    );

                // Begins an asynchronous operation to accept an attempt 
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }
            catch (Exception ex) { Trace.WriteLine(ex.ToString()); }
        }

        public void StopListening()
        {
            //throw new NotImplementedException();//TODO spr czy wystarczy zamknac nasliuchiwanie, zrobic dispose strumieni, usunac niepotrzebne obiekty
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            //TODO przepisac ladnie i wydzielic mniejsze funkcje

            try
            {
                Interlocked.Increment(ref openConnectionsCount);
                // Fetch a user-defined object that contains information 
                object[] obj = new object[2];
                obj = (object[]) ar.AsyncState;

                // Received byte array 
                byte[] buffer = (byte[]) obj[0];

                // A Socket to handle remote host communication. 
                handler = (Socket) obj[1];

                // Received message 
                string content = string.Empty;


                // The number of bytes received. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    content += Encoding.UTF8.GetString(buffer, 0,
                        bytesRead);

                    

                    Trace.WriteLine(content);
                    string buff = "Odebrałem wiadomość od Klienta";
                    byte[] bytes = Encoding.UTF8.GetBytes(buff);
                    //byte[] bytes = new byte[buff.Length * sizeof(char)];
                    //System.Buffer.BlockCopy(buff.ToCharArray(), 0, bytes, 0, bytes.Length);

                    handler.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), handler);
                    Interlocked.Decrement(ref openConnectionsCount);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Interlocked.Decrement(ref openConnectionsCount);
            }
        }

        public IList<SolveRequestMessage> GetUnfinishedTasks()
        {
            lock (solveRequestMessagesLock)
            {
                return solveRequestMessageQueue.ToList();
            }
        }
        
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }

        public void ReceiveAllMessages()
        {
            while(openConnectionsCount > 0)
                Thread.Sleep(1000);
        }
    }
}