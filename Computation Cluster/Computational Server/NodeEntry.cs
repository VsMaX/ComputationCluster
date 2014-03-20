using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computational_Server
{
    public class NodeEntry
    {
        public string IP { get; set; }
        public int ID { get; set; }
        public List<ProblemType> SolvingProblems { get; set; }
    }
}
