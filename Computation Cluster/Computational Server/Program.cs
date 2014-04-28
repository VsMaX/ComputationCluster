using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;
using log4net;

namespace Computational_Server
{
    class Program
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            _logger.Info("Starting server");
            var computationServer = new ComputationServer(new TimeSpan(0, 0, 10), new CommunicationModule("127.0.0.1", 5555, 5000), 2000);
            computationServer.StartServer();
            Console.ReadKey();
            _logger.Info("Stopping server");
            computationServer.StopServer();
            Console.ReadKey();
        }
    }
}