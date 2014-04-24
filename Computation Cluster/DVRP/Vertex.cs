using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public class Vertex
    {
        public int visitID;
        public int locationID;
        public int depotID;
        public double x;
        public double y;
        public double demand;
        public bool added;
        public bool deleted;
        public bool isDepot = false;
        public double early;
        public double late;
        public double duration;
    }
}
