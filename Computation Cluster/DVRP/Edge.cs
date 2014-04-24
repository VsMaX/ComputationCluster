using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public class Edge
    {
        public Vertex node1;
        public Vertex node2;

        public double distance;

        public double initialDistance;
        public bool isJam;
    }
}
