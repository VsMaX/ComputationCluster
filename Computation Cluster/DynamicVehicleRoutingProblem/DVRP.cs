using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DynamicVehicleRoutingProblem
{
    public class DVRP
    {
        public string FormatVersionNumber { get; set; }

        public string Name { get; set; }

        public ProblemType Type { get; set; }

        public string Comment { get; set; }

        public int NumVistis { get; set; } //Number of pickup and delivery points. Required. Does not include depots.

        [DefaultValue(1)]
        public int NumDepots { get; set; } //Number of depots.

        public int NumVehicles { get; set; } //Maximum number of vehicles available. Required.

        [DefaultValue(1)]
        public int NumCapacities { get; set; }

        [DefaultValue(1)] //[NUM_VISITS + NUM_DEPOTS]
        public int NumLocations { get; set; }

        public double Capacities { get; set; }

        [DefaultValue(1)]
        public double Speed { get; set; }

        [DefaultValue(Double.MaxValue)]
        public double MaxTime { get; set; }

        public EdgeWeightType EdgeWeightType { get; set; }
        public EdgeWeightFormat EdgeWeightFormat { get; set; }
        public Objective Objective { get; set; }


        public Depot[] Depots { get; set; }
        public Client[] Clients { get; set; }
        public Location[] Locations { get; set;}

        public int[] ClientID { get; set; } // pomocnicz tablica wykorzystywana do Divide
        public double[,] distances { get; set; }

        public DVRP()
        {
            // Use the DefaultValue propety of each property to actually set it, via reflection.
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
                if (attr != null)
                {
                    prop.SetValue(this, attr.Value);
                }
            }
        }

        #region Function Equals, Operator == and != 
        public static bool operator ==(DVRP v1, DVRP v2)
        {
            return DVRPComparer.AreObjectsEqual(v1, v2, new string[0] { });
        }

        public static bool operator !=(DVRP v1, DVRP v2)
        {
            return !DVRPComparer.AreObjectsEqual(v1, v2, new string[0] { });
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to Point return false.
            DVRP p = obj as DVRP;
            if ((System.Object)p == null)
                return false;

            // Return true if the fields match:
            return DVRPComparer.AreObjectsEqual(this, p, new string[0]{});
        }

        public bool Equals(DVRP p)
        {
            // If parameter is null return false:
            if ((object)p == null)
                return false;

            // Return true if the fields match:
            return DVRPComparer.AreObjectsEqual(this, p, new string[0] { });
        }
        #endregion




    }

    public enum ProblemType
    {
        VRP = 0,
        PDP
    }

    public enum EdgeWeightType
    {
        EUC_2D = 0, //Euclidean [default] 
        MAN_2D, //Manhattan metric 
        MAX_2D, //Maximum metric 
        EXPLICIT //Matrix later in data 
    }

    public enum EdgeWeightFormat
    {
        //For EDGE_WEIGHT_TYPE EXPLICIT, specifies shape of matrix: 
        FULL_MATRIX = 0, //Full matrix [default] 
        LOWER_TRIANG, //Lower triangle of symetric matrix, diagonal not included. 
        ADJ //Adjancency list  
    }

    public enum Objective
    {
        //The objective function to minimize, with optional parameters. Possible values: 
        [Description("VEH-WEIGHT")]
        VEHWEIGHT, //Minimize number of vehicles, then sum of edge weights 
        WEIGHT = 0, //Minimize sum of edge weights within given number of vehicles [Default] 
        [Description("MIN-MAX-LEN")]
        MINMAXLEN //Minimize the maximum route length. Equivalent to make-span in a job-shop problem.
    }
}
