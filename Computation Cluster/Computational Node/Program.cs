using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Computational_Node
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new ComputationnalNode("127.0.0.1", 5555, 2000);
            if (node.RegisterAtServer())
            {
                node.StartQueueProcessingThread();
                node.StartSendingStatusThread();
            }

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}