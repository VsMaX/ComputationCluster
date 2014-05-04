using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPSolution
    {
        public int index;
        public double pathLen;
        public List<Location> paths;
        public List<double> pathsArrivalsTimes;


        public DVRPSolution() { }

        public DVRPSolution(double pathL)
        {
            this.pathLen = pathL;
        }

        public DVRPSolution(int index, double pathL, List<Location> sol, List<double> arrivals)
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

        public static DVRPSolution Parse(string input, DVRP dvrp)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException(input);

            DVRPSolution instance = new DVRPSolution();
            //var lines = input.Split(new[] { '\n' });
            //int ind = 0;
            //for (int i = 0; i < lines.Length - 1; i++)
            //{
            //    string[] split = DVRPHelper.SplitText(lines[i]);

            //    switch (split[0])
            //    {
            //        case:"SOL":
            //            instance = new List<Location>[int.Parse(split[1])];
            //            instance.pathsArrivalsTimes = new List<double>();
            //            instance.pathLen = double.Parse(split[2]);
            //            break;
            //        case "SOLUTION":
                        
            //            break;
            //        case "PATH":
            //            instance.paths[ind] = new List<Location>();
            //            for (int n = 1; n < split.Length; n++)
            //            {
            //                instance.paths[ind].Add(dvrp.Locations.First(x => x.locationID == int.Parse(split[n])));
            //            }
            //            break;
            //        case "TIMES":
            //            instance.pathsArrivalsTimes[ind] = new List<double>();
            //            for (int n = 1; n < split.Length; n++)
            //            {
            //                instance.pathsArrivalsTimes[ind].Add(double.Parse(split[n]));
            //            }
            //            ind++;
            //            break;
            //    }
            //}
            return instance;
        }
    }
}
