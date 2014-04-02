using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new TaskManager("192.168.110.63", 6666);
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.R:
                    node.RegisterAtServer();
                    break;
                case ConsoleKey.S:
                    node.SendStatus();
                    break;
            }

            node.DivideProblem();
            Console.ReadKey();
        }
    }
}