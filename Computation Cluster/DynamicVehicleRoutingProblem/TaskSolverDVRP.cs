using Communication_Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DynamicVehicleRoutingProblem
{
    public class TaskSolverDVRP : TaskSolver
    {
        private DVRP Dvrp;

        public TaskSolverDVRP(byte[] problemData)
            : base(problemData)
        {
            this._problemData = problemData;
            this.Dvrp = DVRP.Parse(CommunicationModule.ConvertDataToString(problemData, problemData.Length));
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var result = new byte[threadCount][];

            var subsets = TaskSolverDVRP.GetAllPartitions<int>(Dvrp.ClientID).ToArray();
            Trace.WriteLine(subsets.Count().ToString());

            int size = subsets.Count() / threadCount;
            Trace.WriteLine(size.ToString());

            for (int i = 1; i <= threadCount; i++) //threadCount
            {
                //Trace.WriteLine("i = " + i.ToString());
                int[][][] tab = new int[size][][];
                if (i == threadCount) // ostatni podzial
                {
                    tab = new int[subsets.Count() - size * i][][];
                    size = tab.Count();
                }
                int ind =0;
                for (int j = (i - 1) * size; j < i * size; j++, ind++)
                {
                    tab[ind] = new int[subsets[j].Length][];
                    for (int k = 0; k < subsets[j].Length; k++)
                    {
                        tab[ind][k] = new int[subsets[j][k].Length];
                        tab[ind][k] = subsets[j][k];
                    }
                }

                //Trace.Write(Client.ClientsToString(tab));
                result[i - 1] = CommunicationModule.ConvertStringToData(Client.ClientsToString(tab));
            }

            return result;
        }

        public override event UnhandledExceptionEventHandler ErrorOccured;

        public override void MergeSolution(byte[][] solutions)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return "DVRP"; }
        }

        public override event TaskSolver.ComputationsFinishedEventHandler ProblemDividingFinished;

        public override event TaskSolver.ComputationsFinishedEventHandler ProblemSolvingFinished;

        public override event TaskSolver.ComputationsFinishedEventHandler SolutionsMergingFinished;


        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {

            var tsp = new Backtracking(partialData, this.Dvrp);
            //result.Clients = tsp.Sortuj(result.Clients)[0];

            tsp.FindCycle(0, 0, 0, this.Dvrp.Capacities, 0);
            List<Location> rozwiazanie = tsp.best_cycle;
            return partialData;
        }
  

        public static IEnumerable<T[][]> GetAllPartitions<T>(T[] elements)
        {
            return TaskSolverDVRP.GetAllPartitions(new T[][] { }, elements);
        }

        private static IEnumerable<T[][]> GetAllPartitions<T>(
            T[][] fixedParts, T[] suffixElements)
        {
            // A trivial partition consists of the fixed parts
            // followed by all suffix elements as one block
            yield return fixedParts.Concat(new[] { suffixElements }).ToArray();

            // Get all two-group-partitions of the suffix elements
            // and sub-divide them recursively
            var suffixPartitions = GetTuplePartitions(suffixElements);
            foreach (Tuple<T[], T[]> suffixPartition in suffixPartitions)
            {
                var subPartitions = GetAllPartitions(
                    fixedParts.Concat(new[] { suffixPartition.Item1 }).ToArray(),
                    suffixPartition.Item2);
                foreach (var subPartition in subPartitions)
                {
                    yield return subPartition;
                }
            }
        }

        private static IEnumerable<Tuple<T[], T[]>> GetTuplePartitions<T>(
            T[] elements)
        {
            // No result if less than 2 elements
            if (elements.Length < 2) yield break;

            // Generate all 2-part partitions
            for (int pattern = 1; pattern < 1 << (elements.Length - 1); pattern++)
            {
                // Create the two result sets and
                // assign the first element to the first set
                List<T>[] resultSets = {
                    new List<T> { elements[0] }, new List<T>() };
                // Distribute the remaining elements
                for (int index = 1; index < elements.Length; index++)
                {
                    resultSets[(pattern >> (index - 1)) & 1].Add(elements[index]);
                }

                yield return Tuple.Create(
                    resultSets[0].ToArray(), resultSets[1].ToArray());
            }
        }
    }

    class Backtracking
    {
        public double ca;
        private byte[] partialData;
        public List<Location> act_cycle;
        public List<Location> best_cycle = null;
        public bool[] used;
        public double? best = null;
        public DVRP result;
        public int ilosc_sciezek;
        public int kk = 12;

        public Backtracking(byte[] partialData, DVRP result)
        {
            act_cycle = new List<Location>();
            //act_cycle.Add(result.Locations[result.Depots[0].locationID]);

            this.partialData = partialData;
            this.result = result;
            used = new bool[result.Clients.Length];
            ca = result.Capacities;
        }

        public void FindCycle(int v, int k, double d, double capacity, double poprzednie)
        {
            if (d >= best || capacity < 0) { capacity -= poprzednie; return; }
            if (capacity == 0)
            {
                capacity = result.Capacities;
                act_cycle.Add(result.Locations[result.Depots[0].locationID]);
                kk++;
                double dist = result.distances[v, result.Locations[result.Depots[0].locationID].locationID];
                //FindCycle(result.Locations[result.Depots[0].locationID].locationID, k + 1, d + dist, capacity);
            }
            if (k == kk)
            {
                best = d;
                best_cycle = act_cycle;
                ilosc_sciezek++;
                return;
            }

            for (int i = 0; i < result.Locations.Length - 1; i++)
            {
                if (!used[result.Locations[i].locationID])
                {
                    used[result.Locations[i].locationID] = true;
                    if (act_cycle.Count > k)
                        act_cycle[k] = result.Locations[i];
                    else
                        act_cycle.Add(result.Locations[i]);
                    double distance = result.distances[i, i + 1];
                    poprzednie = result.Clients[i].size;
                    capacity += poprzednie;
                    FindCycle(result.Locations[i].locationID, k + 1, d + distance, capacity, poprzednie);
                    used[result.Locations[i].locationID] = false;
                }
            }
        }
    }
}
