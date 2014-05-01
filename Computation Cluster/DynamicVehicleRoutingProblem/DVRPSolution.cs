using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPSolution
    {
        public double pathLen;
        public List<Location>[] paths;
        public List<double>[] pathsArrivalsTimes;

        public DVRPSolution() { }

        public DVRPSolution(double pathL)
        {
            this.pathLen = pathL;
        }

        public DVRPSolution(double pathL, List<Location>[] sol, List<double>[] arrivals)
        {
            this.pathLen = pathL;
            this.paths = sol;
            this.pathsArrivalsTimes = arrivals;
        }

        public override string ToString()
        {
            string result = "SOLUTION:" + paths.Length.ToString() + ":" + pathLen + "\n";
            string locations = "";
            string times = "";
            for (int i = 0; i < paths.Length; i++)
            {
                locations += "PATH:";
                times += "TIMES:";
                for (int j = 0; j < paths[i].Count; j++)
                {
                    locations += paths[i][j].ToString() + " ";
                    times += pathsArrivalsTimes[i][j].ToString() + " ";
                }
                result += locations + "\n";
                result += times +"\n";
                locations = "";
                times = "";
            }

            return result;
        }

        public static DVRPSolution Parse(string input, DVRP dvrp)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            DVRPSolution instance = new DVRPSolution();
            var lines = input.Split(new[] { '\n' });
            int ind = 0;
            for (int i = 0; i < lines.Length -1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "SOLUTION":
                        instance.paths = new List<Location>[int.Parse(split[1])];
                        instance.pathsArrivalsTimes = new List<double>[int.Parse(split[1])];
                        instance.pathLen = double.Parse(split[2]);
                        break;
                    case "PATH":
                        instance.paths[ind] = new List<Location>();
                        for (int n = 1; n < split.Length; n++)
                        {
                            instance.paths[ind].Add(dvrp.Locations.First(x=>x.locationID==int.Parse(split[n])));
                        }
                        break;
                    case "TIMES":
                        instance.pathsArrivalsTimes[ind] = new List<double>();
                        for (int n = 1; n < split.Length; n++)
                        {
                            instance.pathsArrivalsTimes[ind].Add(double.Parse(split[n]));
                        }
                        ind++;
                        break;
                }
            }
            return instance;
        }
    }
}
