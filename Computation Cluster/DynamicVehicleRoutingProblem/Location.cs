using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class Location
    {
        public int locationID;
        public double x;
        public double y;



        public static bool operator ==(Location v1, Location v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return false;
                else if (v1.x != v2.x)
                    return false;
                else if (v1.y != v2.y)
                    return false;
                else
                    return true;
            }
            else if (v1 == null && v2 == null)
                return true;
            else
                return false;
        }

        public static bool operator !=(Location v1, Location v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return true;
                else if (v1.x != v2.x)
                    return true;
                else if (v1.y != v2.y)
                    return true;
                else
                    return false;
            }
            else if (v1 == null && v2 == null)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            return this.locationID.ToString();
        }
    }
}
