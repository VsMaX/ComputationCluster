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
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist;
        }

        public static string[] SplitText(string text)
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

        public static int[][] ParsePartialProblemData(byte[] data)
        {
            int[][] result = new int[0][];
            string text = Communication_Library.CommunicationModule.ConvertDataToString(data, data.Length);
            string[] lines = text.Split(new[] { '\n' });

            int set = 0;
            int indeks = 0;
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "NUMSETS":
                        result = new int[int.Parse(split[1])][];
                        indeks = int.Parse(split[2]);
                        break;
                    case "SET":
                        set = int.Parse(split[1]);
                        result[set] = new int[int.Parse(split[2])];
                        //set++;
                        break;
                    default:
                        for (int j = 0; j < split.Length; j++)
                        {
                            result[set][j] = int.Parse(split[j]);
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
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "VRPTEST":
                        instance.FormatVersionNumber = split[1];
                        break;
                    case "COMMENT":
                        instance.Comment += lines[i].Substring(9, lines[i].Length - 9).Trim(new Char[] { ' ', '\r' });// split[j] + " ";
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
                            string[] clientsSplit = DVRPHelper.SplitText(lines[++i]);
                            instance.Clients[j].visitID = int.Parse(clientsSplit[0]);
                            instance.Clients[j].size = int.Parse(clientsSplit[1]);
                            instance.ClientID[j] = j;//int.Parse(clientsSplit[0]);
                        }
                        break;
                    case "LOCATION_COORD_SECTION":
                        for (int j = 0; j < instance.NumLocations; j++)
                        {
                            instance.Locations[j] = new Location();
                            string[] locationsSplit = DVRPHelper.SplitText(lines[++i]);
                            instance.Locations[j].locationID = int.Parse(locationsSplit[0]);
                            instance.Locations[j].x = int.Parse(locationsSplit[1]);
                            instance.Locations[j].y = int.Parse(locationsSplit[2]);
                        }
                        break;
                    case "DEPOT_LOCATION_SECTION":
                        for (int j = 0; j < instance.NumDepots; j++)
                        {
                            string[] depotLocationsSplit = DVRPHelper.SplitText(lines[++i]);
                            int depotId = int.Parse(depotLocationsSplit[0]);
                            instance.Depots.First(x => x.depotID == depotId).locationID = int.Parse(depotLocationsSplit[1]);
                        }
                        break;
                    case "VISIT_LOCATION_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            string[] clientsSplit = DVRPHelper.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x => x.visitID == visitId).locationID = int.Parse(clientsSplit[1]);
                        }
                        break;
                    case "DURATION_SECTION":
                        for (int j = 0; j < instance.NumVistis; j++)
                        {
                            string[] clientsSplit = DVRPHelper.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x => x.visitID == visitId).unld = double.Parse(clientsSplit[1]);
                        }
                        break;
                    case "DEPOT_TIME_WINDOW_SECTION":
                        for (int j = 0; j < instance.NumDepots; j++)
                        {
                            string[] depotLocationsSplit = DVRPHelper.SplitText(lines[++i]);
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
                            string[] clientsSplit = DVRPHelper.SplitText(lines[++i]);
                            int visitId = int.Parse(clientsSplit[0]);
                            instance.Clients.First(x => x.visitID == visitId).time = double.Parse(clientsSplit[1]);
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
                    instance.distances[j, k] = DVRPHelper.Distance(instance.Locations[j], instance.Locations[k]);
                }
            }

            return instance;
        }

        internal static bool CompareArrays(int[] p1, int[] p2)
        {
            if (p1.Length != p2.Length)
                return false;
            for (int i=0; i<p2.Length; i++)
            {
                if (p1[i] != p2[i])
                    return false;
            }
            return true;
        }
    }
}
