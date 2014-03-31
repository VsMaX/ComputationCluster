using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            var computationServer = new ComputationServer("192.168.111.154", 22222, new TimeSpan(0,0,30));
            computationServer.StartListening();
            Trace.WriteLine("Server stopping");
            computationServer.StopListening();
            Trace.WriteLine("Server stopped");
            Console.ReadKey();
        }
    }
}