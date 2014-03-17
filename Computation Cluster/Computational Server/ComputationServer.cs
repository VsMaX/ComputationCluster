using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication_Library;
using Copmutational_Client;
using SuperSocket.SocketBase;

namespace Computational_Server
{
    public class ComputationServer
    {
        private int port;
        private object queueLock = new object();
        private Queue<Problem> problemsQueue;
        private AppServer appServer;

        public ComputationServer(int _port)
        {
            port = _port;
            appServer = new AppServer();
            if (!appServer.Setup(port)) //Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }
        }

        public void StartListening()
        {

            Console.WriteLine();

            //Try to start the appServer
            if (!appServer.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The server started successfully, press key 'q' to stop it!");
            appServer.NewSessionConnected += new SessionHandler<AppSession>(appServer_NewSessionConnected);
            appServer.NewRequestReceived += appServer_NewRequestReceived;
            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            //Stop the appServer
            
            Console.WriteLine("The server was stopped!");
            Console.ReadKey();
        }

        private void appServer_NewRequestReceived(AppSession session, SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }

        private void appServer_NewSessionConnected(AppSession session)
        {
            session.Send("Message!!!!!!");
        }

        public void StopListening()
        {
            appServer.Stop();
        }
    }
}
