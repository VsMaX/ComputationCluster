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

        public int NodeNumber;
        public List<double> PartialPathLen;
        public List<Location>[] PartialPaths;
        public List<double>[] PartialPathsArrivalsTimes;
        public List<int>[] PartialClientID;
        public int ElemCount;

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

        public DVRPPartialSolution(List<int> node, List<int> ind, DVRPPartialSolution[] ps, double pathLen) // do ostaecznego rozwiazania
        {
            this.pathLen = pathLen;
            this.PartialPathLen = new List<double>();
            this.PartialPaths = new List<Location>[node.Count];
            this.PartialPathsArrivalsTimes = new List<double>[node.Count];
            for (int i = 0; i < node.Count; i++)
            {
                this.PartialPathLen.Add(ps[node[i]].PartialPathLen[ind[i]]);
                this.PartialPaths[i] = new List<Location>(ps[node[i]].PartialPaths[ind[i]]);
                this.PartialPathsArrivalsTimes[i] = new List<double>(ps[node[i]].PartialPathsArrivalsTimes[ind[i]]);
            }
 
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
            //instance.ElemCount = new List<int>();

            var lines = input.Split(new[] { '\n' });
            int ind = 0;
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "SOL":
                        instance.ElemCount = int.Parse(split[1]);
                        instance.PartialPaths = new List<Location>[int.Parse(split[1])];
                        instance.PartialPathsArrivalsTimes = new List<double>[int.Parse(split[1])];
                        instance.PartialPathLen = new List<double>();
                        instance.PartialClientID = new List<int>[int.Parse(split[1])];
                        instance.NodeNumber = int.Parse(split[2]);
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

        internal static string SolutionToString(DVRPPartialSolution solution)
        {

            string result = "SOLUTION:" + solution.PartialPaths.Length + ":" + solution.pathLen + "\n";

            string locations = "";
            string times = "";
            string pathslen = "";
            for (int i = 0; i < solution.PartialPaths.Length; i++)
            {
                locations = "PATH:";// + solution.PartialPathLen[i].ToString();

                for (int j = 0; j < solution.PartialPaths[i].Count(); j++)
                    locations += solution.PartialPaths[i][j].ToString() + " ";
                result += locations + "\n";

                times = "TIMES:";

                for (int j = 0; j < solution.PartialPathsArrivalsTimes[i].Count(); j++)
                    times += solution.PartialPathsArrivalsTimes[i][j].ToString() + " ";
                result += times + "\n";

                pathslen = "PATHLEN:";// + solution.PartialPathLen[i].ToString();
                pathslen += solution.PartialPathLen[i].ToString() + " ";
                result += pathslen + "\n";

            }
            return result;
        }
        public static DVRPPartialSolution Parse2FinalSol(string input, DVRP dvrp)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            DVRPPartialSolution instance = new DVRPPartialSolution();
            //instance.ElemCount = new List<int>();

            var lines = input.Split(new[] { '\n' });
            int ind = 0;
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "SOLUTION":
                        instance.PartialPaths = new List<Location>[int.Parse(split[1])];
                        instance.PartialPathsArrivalsTimes = new List<double>[int.Parse(split[1])];
                        instance.PartialPathLen = new List<double>();
                        instance.pathLen = double.Parse(split[2]);
                        break;
                    case "PATH":
                        //instance.PartialPathLen.Add(double.Parse(split[1]));
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
                    case "PATHLEN":
                        for (int n = 1; n < split.Length; n++)
                        {
                            instance.PartialPathLen.Add(double.Parse(split[n]));
                        }
                        ind++;
                        break;
                }
            }
            return instance;
        }

    }
}


