using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Node
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new ComputationnalNode("127.0.0.1", 22222);
            Console.WriteLine("Press R to register node and S to send status message");
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.S:
                    node.SendStatus();
                    break;
                case ConsoleKey.R:
                    node.RegisterAtServer();
                    break;
                default:
                    node.Disconnect();
                    break;
            }
            Console.ReadKey();
        }
    }
}