using System;
using System.Collections.Generic;
using System.Configuration;
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
            int receiveDataTimeout = Int32.Parse(ConfigurationManager.AppSettings["ReceiveDataTimeout"]);
            var node = new TaskManager("127.0.0.1", 5555, receiveDataTimeout);
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.R:
                    node.Start();
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