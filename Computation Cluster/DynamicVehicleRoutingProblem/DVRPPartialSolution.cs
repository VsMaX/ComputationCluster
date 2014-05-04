using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPPartialSolution
    {
        public int index;
        public double pathLen;
        public List<Location> paths;
        public List<double> pathsArrivalsTimes;
        public List<int> ClientID;

        public List<double> PartialPathLen;
        public List<Location>[] PartialPaths;
        public List<double>[] PartialPathsArrivalsTimes;
        public List<int>[] PartialClientID;
        public DVRPPartialSolution() { }

        public DVRPPartialSolution(double pathL)
        {
            this.pathLen = pathL;
        }

        public DVRPPartialSolution(int index, double pathL, List<Location> sol, List<double> arrivals)
        {
            this.index = index;
            this.pathLen = pathL;
            this.paths = sol;
            this.pathsArrivalsTimes = arrivals;
        }

        public override string ToString()
        {
            string result = "SOLUTION:" + index + ":" + pathLen + "\n";
            string locations = "";
            string times = "";
            locations += "PATH:";
            for (int i = 0; i < paths.Count; i++)
            {
                locations += paths[i].locationID.ToString() + " ";
            }
            result += locations + "\n";
            times += "TIMES:";

            for (int i = 0; i < paths.Count; i++)
            {
                times += pathsArrivalsTimes[i].ToString() + " ";
            }
            result += times + "\n";


            return result;
        }

        public static DVRPPartialSolution Parse(string input, DVRP dvrp)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            DVRPPartialSolution instance = new DVRPPartialSolution();
            var lines = input.Split(new[] { '\n' });
            int ind = 0;
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "SOL":
                        instance.PartialPaths = new List<Location>[int.Parse(split[1])];
                        instance.PartialPathsArrivalsTimes = new List<double>[int.Parse(split[1])];
                        instance.PartialPathLen = new List<double>();
                        instance.PartialClientID = new List<int>[int.Parse(split[1])];

                        //instance.PartialClientID = new List<int>[int.Parse(split[1])];
                       // instance.PartialSetIndex = new List<double>();
                        break;
                    case "SET":
                        i++;
                        split = DVRPHelper.SplitText(lines[i]);
                        instance.PartialClientID[ind] = new List<int>();
                        for (int n = 0; n < split.Length; n++)
                        { 
                            instance.PartialClientID[ind].Add(int.Parse(split[n]));
                        }
                        ind++;
                        break;
                    case "SOLUTION":
                        //instance.PartialPaths = new List<Location>[int.Parse(split[1])];
                        //instance.PartialPathsArrivalsTimes = new List<double>[int.Parse(split[1])];
                        instance.PartialPathLen.Add(Double.Parse(split[2]));
                        break;
                    case "PATH":
                        instance.PartialPaths[ind] = new List<Location>();
                        for (int n = 1; n < split.Length; n++)
                        {
                            if (int.Parse(split[n]) != -1)
                                instance.PartialPaths[ind].Add(dvrp.Locations.First(x => x.locationID == int.Parse(split[n])));
                            else
                                instance.PartialPaths[ind].Add(new Location() { locationID = -1 });
                        }
                        break;
                    case "TIMES":
                        instance.PartialPathsArrivalsTimes[ind] = new List<double>();
                        for (int n = 1; n < split.Length; n++)
                        {
                            instance.PartialPathsArrivalsTimes[ind].Add(double.Parse(split[n]));
                        }
                        
                        break;
                }
            }
            return instance;
        }

        internal static string ArrayToString(int[] p)
        {
            string result = "SET:" + p.Length.ToString() + "\n";
            for (int i = 0; i < p.Length; i++)
            {
                result += p[i].ToString() + " ";
            }
            return result+"\n";
        }
    }
}


