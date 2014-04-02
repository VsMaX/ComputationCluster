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
            var node = new ComputationnalNode("192.168.110.63", 6666);
            Console.WriteLine("Press S to send status message");
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.S:
                    node.SendStatus();
                    break;
                default:
                    node.Disconnect();
                    break;
            }
            Console.ReadKey();
        }
    }
}