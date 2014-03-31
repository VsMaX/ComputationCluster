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
            var node = new TaskManager("127.0.0.1", 22222);
            Console.ReadKey();
            node.RegisterAtServer();
            Console.ReadKey();
            //node.ReceiveDataFromServer();
        }
    }
}
