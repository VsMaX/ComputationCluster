using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public class DVRP
    {
        public string Name { get; set; }
        public ProblemType Type { get; set; }
        public string Comment { get; set; }
        public int NumVistis { get; set; } //Number of pickup and delivery points. Required. Does not include depots.
        public int NumDepots { get; set; } //Number of depots.
        public int NumVehicles { get; set; } //Maximum number of vehicles available. Required.
        public int NumCapacities { get; set; }
        public int NumLocations { get; set; }
        public double Capacities { get; set; }
        public double Speed { get; set; }
        public double MaxTime { get; set; }
        public EdgeWeightType EdgeWeightType { get; set; }
        public EdgeWeightFormat EdgeWeightFormat { get; set; }
        public Objective Objective { get; set; }


        public static DVRP Parse(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            var instance = new DVRP();

            

            return instance;
        }

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
