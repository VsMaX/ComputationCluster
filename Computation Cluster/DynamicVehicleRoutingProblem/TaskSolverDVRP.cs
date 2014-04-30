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
    //[MethodBoundary]
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
                int ind = 0;
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
            int[][][] partial = DVRP.ParseData(partialData);
            double bestPathLength = Double.MaxValue;
            List<Location>[] actualSol = new List<Location>[0];
            List<Location>[] bestSol = new List<Location>[0];
            int setindex;
            int pathindex;
            for (int set = 0; set < partial.Length; set++)
            {
                double actualPathLength = 0;
                actualSol = new List<Location>[partial[set].Length];
                for (int path = 0; path < partial[set].Length; path++)
                {
                    var tsp = new Backtracking(partial[set][path], this.Dvrp);

                    tsp.FindCycle(0, 0, 0, 0, 0);//this.Dvrp.Capacities);

                    if (tsp.best_cycle != null)
                        actualSol[path] = new List<Location>(tsp.best_cycle);
                    actualPathLength += tsp.best;
                    pathindex = path;
                }
                if (actualPathLength < bestPathLength)
                {
                    bestPathLength = actualPathLength;
                    bestSol = new List<Location>[actualSol.Length];
                    for (int i = 0; i < actualSol.Length; i++)
                    {
                        bestSol[i] = new List<Location>(actualSol[i]);
                    }
                    setindex = set;

                }
            }
            Trace.WriteLine(bestPathLength.ToString());
            double dl = 0;
            for (int i = 0; i < bestSol.Length; i++)
                for (int j = 1; j < bestSol[i].Count; j++)
                {
                    dl += DVRPHelper.Distance(bestSol[i][j - 1], bestSol[i][j]);
                }
            return CommunicationModule.ConvertStringToData(bestPathLength.ToString());
        }

        #region PARTITION OF PROBLEM
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
        #endregion
    }

    public class Backtracking
    {
        //public double ca;
        public int[] clientsId; // INDEKSY TABLICOWE
        //private byte[] partialData;
        public List<Location> act_cycle;
        public List<Location> best_cycle = null;
        public bool[] used;
        public double best = Double.MaxValue;
        public DVRP dvrp;
        public double cutOff = 0.5;
        //public int ilosc_sciezek;
        public int kk;

        public Backtracking(int[] partialData, DVRP dvrp)
        {
            act_cycle = new List<Location>();
            //act_cycle.Add(result.Locations[result.Depots[0].locationID]);

            this.clientsId = partialData;
            this.dvrp = dvrp;
            used = new bool[partialData.Length];
            //ca = dvrp.Capacities;
            kk = partialData.Length;
        }

        public void FindCycle(int v, int k, double d, double time, double capacity)
        {
            if (d >= best || time > dvrp.Depots[0].end) { return; }//|| time > dvrp.Depots[0].end
            if (k == kk)
            {
                double distToDepot = DVRPHelper.Distance(act_cycle[act_cycle.Count - 1], act_cycle[0]);

                if (d + distToDepot < best)
                {
                    best = d + distToDepot;
                    act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                    best_cycle = new List<Location>(act_cycle);
                    act_cycle.RemoveAt(act_cycle.Count - 1);
                }
                return;
            }


            for (int i = 0; i < clientsId.Length; i++)
            {
                if (!used[i])
                {
                    used[i] = true;

                    if (capacity + dvrp.Clients[clientsId[i]].size > 0)
                    {
                        double dist = dvrp.distances[v, dvrp.Clients[clientsId[i]].locationID];
                        double t = dist / dvrp.Speed + dvrp.Clients[clientsId[i]].unld;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                        FindCycle(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID].locationID, k + 1, d + dist, time + t + tWait, capacity + dvrp.Clients[clientsId[i]].size);
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                    }
                    else if (capacity + dvrp.Clients[clientsId[i]].size == 0)
                    {
                        double dist = dvrp.distances[v, dvrp.Clients[clientsId[i]].locationID];
                        dist += dvrp.distances[dvrp.Clients[clientsId[i]].locationID, dvrp.Depots[0].locationID];
                        double t = dist / dvrp.Speed + dvrp.Clients[clientsId[i]].unld;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                        act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                        kk++;
                        FindCycle(0, k + 2, d + dist, time + t + tWait, dvrp.Capacities);
                        kk--;
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                    }
                    else
                    {
                        double dist = 0;
                        if (act_cycle.Count > 0)
                        {
                            dist = dvrp.distances[act_cycle[act_cycle.Count - 1].locationID, dvrp.Depots[0].locationID];
                        }
                        dist += dvrp.distances[dvrp.Depots[0].locationID, dvrp.Clients[clientsId[i]].locationID];

                        double t = dist / dvrp.Speed;
                        double tWait = 0;//czas oczekiwania na przyjscie zgłoszenia
                        if (dvrp.Clients[clientsId[i]].time < cutOff * dvrp.Depots[0].end && time < dvrp.Clients[clientsId[i]].time)
                            tWait = dvrp.Clients[clientsId[i]].time - time;
                        act_cycle.Add(dvrp.Locations[dvrp.Depots[0].locationID]);
                        kk++;
                        act_cycle.Add(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                        //act_cycle.Insert(act_cycle.Count - 1, dvrp.Locations[dvrp.Depots[0].locationID]);

                        FindCycle(dvrp.Clients[clientsId[i]].locationID, k + 2, d + dist, time + t + tWait, dvrp.Capacities + dvrp.Clients[clientsId[i]].size);
                        kk--;
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                        act_cycle.RemoveAt(act_cycle.Count - 1);
                    }
                    used[i] = false;
                    //capacity -= dvrp.Clients[i].size;
                    //act_cycle.Remove(dvrp.Locations[dvrp.Clients[clientsId[i]].locationID]);
                }
            }
        }
    }
}