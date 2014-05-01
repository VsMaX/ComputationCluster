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
            this.Dvrp = DVRPHelper.Parse(CommunicationModule.ConvertDataToString(problemData, problemData.Length));
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var result = new byte[threadCount][];

            var subsets = TaskSolverDVRP.GetAllPartitions<int>(Dvrp.ClientID).ToArray();
            Trace.WriteLine(subsets.Count().ToString());

            int size = subsets.Count() / threadCount;
            Trace.WriteLine(size.ToString());

            for (int i = 1; i <= threadCount; i++)
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
            for (int i = 0; i < solutions.Length; i++)
            {
                DVRPSolution sol = DVRPSolution.Parse(CommunicationModule.ConvertDataToString(solutions[i], solutions[i].Length), Dvrp);
            }
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
            int[][][] partial = DVRPHelper.ParsePartialProblemData(partialData);
            double bestPathLength = Double.MaxValue;
            List<Location>[] actualSol = new List<Location>[0];
            List<Location>[] bestSol = new List<Location>[0];
            List<double>[] arrivalsTimes = new List<double>[0];
            List<double>[] bestArrivalsTimes = new List<double>[0];

            for (int set = 0; set < partial.Length; set++)
            {
                double actualPathLength = 0;
                actualSol = new List<Location>[partial[set].Length];
                arrivalsTimes = new List<double>[partial[set].Length];
                for (int path = 0; path < partial[set].Length; path++)
                {
                    DVRPPathFinder pathFinder = new DVRPPathFinder(partial[set][path], this.Dvrp);

                    pathFinder.FindCycle(0, 0, 0, 0, 0);

                    if (pathFinder.best_cycle != null)
                    {
                        actualSol[path] = new List<Location>(pathFinder.best_cycle);
                        arrivalsTimes[path] = new List<double>(pathFinder.bestArrivalsTimes);
                    }
                    actualPathLength += pathFinder.bestPathLen;
                }
                if (actualPathLength < bestPathLength)
                {
                    bestPathLength = actualPathLength;
                    bestSol = new List<Location>[actualSol.Length];
                    bestArrivalsTimes = new List<double>[actualSol.Length];
                    for (int i = 0; i < actualSol.Length; i++)
                    {
                        bestSol[i] = new List<Location>(actualSol[i]);
                        bestArrivalsTimes[i] = new List<double>(arrivalsTimes[i]);
                    }

                }
            }
            DVRPSolution solution = new DVRPSolution(bestPathLength, bestSol, bestArrivalsTimes);
            string solString = solution.ToString();
            return CommunicationModule.ConvertStringToData(solString);
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
}
