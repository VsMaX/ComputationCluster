using System;
using System.Collections.Generic;
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
            var computationServer = new ComputationServer("127.0.0.1", 22222, new TimeSpan(0,0,30));
            computationServer.StartListening();
            computationServer.StopListening();
            Console.ReadKey();
        }
    }
}