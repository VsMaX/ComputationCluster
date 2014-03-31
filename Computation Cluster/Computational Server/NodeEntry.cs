using Communication_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Server
{
    public class NodeEntry
    {
        public int ID { get; set; }
        public RegisterType Type { get; set; }
        public List<string> SolvingProblems { get; set; }
        public DateTime LastActive { get; set; }
    }
}
