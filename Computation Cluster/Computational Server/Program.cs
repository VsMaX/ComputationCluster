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

namespace Computational_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            var computationServer = new ComputationServer(new TimeSpan(0,0,10), new CommunicationModule("127.0.0.1", 5555, 300000));
            computationServer.StartServer();
            Console.ReadKey();
            Trace.WriteLine("Server stopping");
            computationServer.StopServer();
            Trace.WriteLine("Server stopped");
            Console.ReadKey();
        }
    }
}