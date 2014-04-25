using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class Client
    {
        public int visitID;
        public int locationID;
        public double time;
        public double unld; //unload time
        public double size; //size of request

        public static bool operator ==(Client v1, Client v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return false;
                else if (v1.visitID != v2.visitID)
                    return false;
                else if (v1.time != v2.time)
                    return false;
                else if (v1.size != v2.size)
                    return false;
                else if (v1.unld != v2.unld)
                    return false;
                else
                    return true;
            }
            else if (v1 == null && v2 == null)
                return true;
            else
                return false;
        }

        public static bool operator !=(Client v1, Client v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return true;
                else if (v1.visitID != v2.visitID)
                    return true;
                else if (v1.time != v2.time)
                    return true;
                else if (v1.size != v2.size)
                    return true;
                else if (v1.unld != v2.unld)
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
