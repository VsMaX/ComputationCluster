using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class Depot
    {
        public int depotID;
        public int locationID;
        public double start;//working hours
        public double end;

        public static bool operator ==(Depot v1, Depot v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return false;
                else if (v1.depotID != v2.depotID)
                    return false;
                else if (v1.start != v2.start)
                    return false;
                else if (v1.end != v2.end)
                    return false;
                else
                    return true;
            }
            else if (v1 == null && v2 == null)
                return true;
            else
                return false;
        }

        public static bool operator !=(Depot v1, Depot v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return true;
                else if (v1.depotID != v2.depotID)
                    return true;
                else if (v1.start != v2.start)
                    return true;
                else if (v1.end != v2.end)
                    return true;
                else
                    return false;
            }
            else if (v1 == null && v2 == null)
                return false;
            else
                return true;
        }
    }
}
