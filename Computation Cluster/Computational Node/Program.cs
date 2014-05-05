using System;
using System.Collections.Generic;
using System.IO;
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
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var node = new ComputationalNode("127.0.0.1", 5555, 2000);
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