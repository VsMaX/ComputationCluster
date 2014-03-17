using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication_Library;
using Copmutational_Client;
using ZeroMQ;

namespace Computational_Server
{
    public class ComputationServer
    {
        ZmqContext Context
        {
            get
            {
                if (_context == null)
                    _context = ZmqContext.Create();
                return _context;
            }
            set
            {
                _context = value;
            }
        }
        private ZmqContext _context;

        ZmqSocket Server
        {
            get
            {
                if (_server == null)
                    _server = Context.CreateSocket(SocketType.PUB);
                return _server;
            }
            set
            {
                _server = value;
            }
        }
        private ZmqSocket _server;

        private string ip;
        private string port;

        private object queueLock = new object();

        private Queue<Problem> problemsQueue;

        public ComputationServer(string _ip, string _port)
        {
            ip = _ip;
            port = _port;
        }

        public void StartListening()
        {
            Server.Bind("tcp://" + ip + ":" + port);

            while (true)
            {
                // Wait for next request from client
                string message = Server.Receive(Encoding.Unicode);
                ProcessMessage(message);
                Console.WriteLine("Received request: {0}", message);

                // Do Some 'work'
                Thread.Sleep(1000);

                // Send reply back to client
                Server.Send("World", Encoding.Unicode);
            }
        }

        private void ProcessMessage(string message)
        {
            
        }
    }
}
