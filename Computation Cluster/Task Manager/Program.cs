using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var node = new TaskManager("127.0.0.1", 5555);
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.R:
                    node.StartTM();
                    break;
                case ConsoleKey.S:
                    //node.SendStatus();
                    break;
            }
            //node.DivideProblem();
            Console.ReadKey();
        }
    }
}