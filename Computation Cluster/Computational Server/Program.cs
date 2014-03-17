using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            var computationServer = new ComputationServer(5679);
        }
    }
}
