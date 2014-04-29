using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPHelper
    {
        public static double Distance(Location l1, Location l2)
        {
            double dx = l1.x - l2.x;
            double dy = l1.y - l2.y;
            double dist = Math.Sqrt(dx * dx - dy * dy);
            return dist;
        }
    }
}
