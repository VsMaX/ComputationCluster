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

        private static string[] SplitText(string text)
        {
            string[] splitedText = text.Split(new Char[] { ' ', ':', '\t' });
            char[] charsToTrim = { '\r', ' ' };
            List<string> splitList = new List<string>();
            foreach (var s in splitedText)
            {
                string result = s.Trim(charsToTrim);
                if (result != "")
                    splitList.Add(result);
            }
            return splitList.ToArray();
        }

        public static int[][][] ParseData(byte[] data)
        {
            int[][][] result = new int[0][][];
            string text = Communication_Library.CommunicationModule.ConvertDataToString(data, data.Length);
            string[] lines = text.Split(new[] {'\n'});

            int set = 0;
            int path = 0;
            for (int i = 0; i < lines.Length-1; i++)
            {
                string[] split = DVRP.SplitText(lines[i]);

                switch (split[0])
                {
                    case "NUMSETS":
                        result = new int[int.Parse(split[1])][][];
                        break;
                    case "SET":
                        result[set] = new int[int.Parse(split[1])][];
                        set++;
                        path = 0;
                        break;
                    case "PATH":
                        result[set - 1][path] = new int[int.Parse(split[1])];
                        path++;
                        break;
                    default:
                        for (int j = 0; j < split.Length; j++)
                        {
                            result[set - 1][path - 1][j] = int.Parse(split[j]); 
                        }
                        break;
                }

            }
            return result;
        }

        public static DVRP Parse(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            var instance = new DVRP();
            var lines = input.Split(new[] { '\n' });

            for (int i = 0; i < lines.Length; i++)
            {
                string[] split = DVRP.SplitText(lines[i]);

                switch (split[0])
                {
                    case "VRPTEST":
                        instance.FormatVersionNumber = split[1];
                        break;
                    case "COMMENT":
                        instance.Comment += lines[i].Substring(9, lines[i].Length - 9).Trim(new Char[] { ' ','\r' });// split[j] + " ";
                        break;
                    case "NAME":
                        instance.Name = split[1];
                        break;
                    case "NUM_DEPOTS":
                        instance.NumDepots = int.Parse(split[1]);
                        instance.Depots = new Depot[instance.NumDepots];
                        break;
                    case "NUM_CAPACITIES":
                        instance.NumCapacities = int.Parse(split[1]);
                        break;
                    case "NUM_VISITS":
                        instance.NumVistis = int.Parse(split[1]);
                        instance.Clients = new Client[instance.NumVistis];
                        instance.ClientID = new int[instance.NumVistis];
                        break;
                    case "NUM_LOCATIONS":
                        instance.NumLocations = int.Parse(split[1]);
                        instance.Locations = new Location[instance.NumLocations];
                        break;
                    case "NUM_VEHICLES":
                        instance.NumVehicles = int.Parse(split[1]);
                        break;
                    case "CAPACITIES":
                        instance.Capacities = int.Parse(split[1]);
                        break;
                    //case "DATA_SECTION":
                        
                    case "DEPOTS":
                        for (int j = 0; j < instance.NumDepots; j++)
                        {
                            instance.Depots[j] = new Depot();
                            instance.Depots[j].depotID = int.Parse(lines[++i]);
                        }
                        break;
                    case "DEMAND_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            instance.Clients[j] = new Client();
                            string[] clientsSplit = DVRP.SplitText(lines[++i]);
                            instance.Clients[j].visitID = int.Parse(clientsSplit[0]);
                            instance.Clients[j].size = int.Parse(clientsSplit[1]);
                            instance.ClientID[j] = j;//int.Parse(clientsSplit[0]);
                        }
                        break;
                    case "LOCATION_COORD_SECTION":
                        for (int j = 0; j < instance.NumLocations; j++)
                        {
                            instance.Locations[j] = new Location();
                            string[] locationsSplit = DVRP.SplitText(lines[++i]);
                            instance.Locations[j].locationID = int.Parse(locationsSplit[0]);
                            instance.Locations[j].x = int.Parse(locationsSplit[1]);
                            instance.Locations[j].y = int.Parse(locationsSplit[2]);
                        }
                        break;
                    case "DEPOT_LOCATION_SECTION":
                        for (int j = 0; j < instance.NumDepots; j++)
                        {
                            string[] depotLocationsSplit = DVRP.SplitText(lines[++i]);
                            int depotId = int.Parse(depotLocationsSplit[0]);
                            instance.Depots.First(x => x.depotID == depotId).locationID = int.Parse(depotLocationsSplit[1]);
                        }
                        break;
                    case "VISIT_LOCATION_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            string[] clientsSplit = DVRP.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x=>x.visitID==visitId).locationID = int.Parse(clientsSplit[1]);
                        }
                        break;
                    case "DURATION_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            string[] clientsSplit = DVRP.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x=>x.visitID==visitId).unld = double.Parse(clientsSplit[1]);
                        }
                        break;
                    case "DEPOT_TIME_WINDOW_SECTION":
                        for (int j = 0; j < instance.NumDepots; j++)
                        {
                            string[] depotLocationsSplit = DVRP.SplitText(lines[++i]);
                            int depotId = int.Parse(depotLocationsSplit[0]);
                            Depot d = instance.Depots.First(x => x.depotID == depotId);
                            d.start = double.Parse(depotLocationsSplit[1]);
                            d.end = double.Parse(depotLocationsSplit[2]);
                        }
                        break;
//COMMENT: TIMESTEP: 7
                    case "TIME_AVAIL_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            string[] clientsSplit = DVRP.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x=>x.visitID==visitId).time = double.Parse(clientsSplit[1]);
                        }
                        break;
                    case "EOF":
                        break;
                }
            }

            instance.distances = new double[instance.Locations.Length, instance.Locations.Length];

            for (int j = 0; j < instance.Locations.Length; j++)
            {
                for (int k = 0; k < instance.Locations.Length; k++)
                {
                    double x = instance.Locations[instance.Locations[j].locationID].x - instance.Locations[instance.Locations[k].locationID].x;
                    double y = instance.Locations[instance.Locations[j].locationID].y - instance.Locations[instance.Locations[k].locationID].y;
                    instance.distances[j, k] = Math.Sqrt(x * x + y * y);
                }
            }

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
