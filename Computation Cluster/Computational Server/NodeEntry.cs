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
        public ulong Id { get; set; }
        public RegisterType Type { get; set; }
        public List<string> SolvingProblems { get; set; }
        public byte ParallelThreads { get; set; }
        public DateTime LastActive { get; set; }

        public NodeEntry(ulong id, RegisterType type, List<string> solvingProblems, byte parallelThreads)
        {
            this.Id = id;
            this.Type = type;
            this.SolvingProblems = solvingProblems;
            this.ParallelThreads = parallelThreads;
            LastActive = DateTime.Now;
        }
    }
}
