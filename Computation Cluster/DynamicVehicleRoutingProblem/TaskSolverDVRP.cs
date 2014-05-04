using Communication_Library;
using System;
using System.Collections;
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
        public DVRP Dvrp;

        public TaskSolverDVRP(byte[] problemData)
            : base(problemData)
        {
            this._problemData = problemData;
            this.Dvrp = DVRPHelper.Parse(CommunicationModule.ConvertDataToString(problemData, problemData.Length));
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            this.State = TaskSolverState.Dividing;

            var result = new byte[threadCount][];

            var subsets = CreateSubsets(Dvrp.ClientID).ToArray(); 
            Trace.WriteLine(subsets.Count().ToString());
            int size = subsets.Length / threadCount;
            Trace.WriteLine(size.ToString());
            int[][] res = new int[threadCount][];
            int indeks = 0;
            for (int i = 1; i <= threadCount; i++)
            {
                int[][] tab = new int[size][];
                if (i == threadCount) // ostatni podzial
                {
                    tab = new int[subsets.Count() - size * i][];
                    size = tab.Count();
                }

                int ind = 0;
                for (int j = (i - 1) * size; j < i * size; j++, ind++)
                {
                    tab[ind] = new int[subsets[j].Length];

                    for (int k = 0; k < subsets[j].Length; k++)
                    {
                        tab[ind] = new int[subsets[j].Length];
                        tab[ind] = subsets[j];
                    }
                }
                
                result[i - 1] = CommunicationModule.ConvertStringToData(Client.ClientsToString(tab, indeks));
                indeks++;
            }

            return result;
        }
        List<T[]> CreateSubsets<T>(T[] originalArray)
        {
            List<T[]> subsets = new List<T[]>();

            for (int i = 0; i < originalArray.Length; i++)
            {
                int subsetCount = subsets.Count;
                subsets.Add(new T[] { originalArray[i] });

                for (int j = 0; j < subsetCount; j++)
                {
                    T[] newSubset = new T[subsets[j].Length + 1];
                    subsets[j].CopyTo(newSubset, 0);
                    newSubset[newSubset.Length - 1] = originalArray[i];
                    subsets.Add(newSubset);
                }
            }

            return subsets;
        }
        public override event UnhandledExceptionEventHandler ErrorOccured;

        public override void MergeSolution(byte[][] solutions)
        {
            this.State = TaskSolverState.Merging;
            Dictionary<List<int>, double> dit = new Dictionary<List<int>, double>();

            for (int i = 0; i < solutions.Length; i++)
            {
                DVRPPartialSolution ps = DVRPPartialSolution.Parse(CommunicationModule.ConvertDataToString(solutions[i],solutions[i].Length), Dvrp);
                //dit.Concat(ps.PartialPathLen).ToDictionary(ps.;
                

            }

            //DVRPSolution bestSol = new DVRPSolution(Double.MaxValue);
            //for (int i = 0; i < solutions.Length; i++)
            //{
            //    DVRPSolution sol = DVRPSolution.Parse(CommunicationModule.ConvertDataToString(solutions[i], solutions[i].Length), Dvrp);
            //    if (sol.pathLen < bestSol.pathLen)
            //    {
            //        bestSol = sol;
            //    }
            //}
            //this.Solution = CommunicationModule.ConvertStringToData(bestSol.ToString());
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
            this.State = TaskSolverState.Solving;
            int[][] partial = DVRPHelper.ParsePartialProblemData(partialData);
            DVRPPartialSolution solution;
            string solString = "SOL:" + partial.Length.ToString() + "\n";
            for (int set = 0; set < partial.Length; set++)
            {
                DVRPPathFinder pathFinder = new DVRPPathFinder(partial[set], this.Dvrp);
                pathFinder.FindCycle(0, 0, 0, 0, 0);
                if (pathFinder.best_cycle != null)
                    solution = new DVRPPartialSolution(set, pathFinder.bestPathLen, pathFinder.best_cycle, pathFinder.bestArrivalsTimes);
                else
                {
                    List<Location> ll = new List<Location>();
                    ll.Add(new Location { locationID = -1 });
                    solution = new DVRPPartialSolution(set, -1, ll, new List<double> { -1 });
                }
                solString += solution.ToString();
            }
            return CommunicationModule.ConvertStringToData(solString);
        }

        #region PARTITION OF PROBLEM
        public static IEnumerable<T[][]> GetAllPartitions<T>(T[] elements) {
            return GetAllPartitions(new T[][]{}, elements);
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
            foreach (Tuple<T[], T[]> suffixPartition in suffixPartitions) {
                var subPartitions = GetAllPartitions(
                    fixedParts.Concat(new[] { suffixPartition.Item1 }).ToArray(),
                    suffixPartition.Item2);
                foreach (var subPartition in subPartitions) {
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
            for (int pattern = 1; pattern < 1 << (elements.Length - 1); pattern++) {
                // Create the two result sets and
                // assign the first element to the first set
                List<T>[] resultSets = {
                    new List<T> { elements[0] }, new List<T>() };
                // Distribute the remaining elements
                for (int index = 1; index < elements.Length; index++) {
                    resultSets[(pattern >> (index - 1)) & 1].Add(elements[index]);
                }

                yield return Tuple.Create(
                    resultSets[0].ToArray(), resultSets[1].ToArray());
            }
        }
        #endregion
    }
}
