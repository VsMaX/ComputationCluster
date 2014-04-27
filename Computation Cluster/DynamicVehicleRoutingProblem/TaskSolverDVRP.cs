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
            var subsets = TaskSolverDVRP.GetAllPartitions<int>(Dvrp.ClientID);
            Trace.WriteLine(subsets.Count().ToString());

            int size = subsets.Count() / threadCount;
            Trace.WriteLine(size.ToString());

            for (int i = 1; i <= 1; i++) //threadCount
            {
                Trace.WriteLine("i = " + i.ToString());
                int[][][] tab = new int[size][][];
                if (i == threadCount) // ostatni podzial
                {
                    tab = new int[subsets.Count() - size * i][][];
                    size = tab.Count();
                }
                for (int j = (i - 1) * size; j < i * size; j++)
                    for (int k = 0; k < subsets.ElementAt(j).Count(); k++)
                    {
                        Trace.WriteLine("j = " + j.ToString() + "  k = " + k.ToString());
                        tab[k] = subsets.ElementAt(j);
                    }

                Trace.Write(Client.ClientsToString(tab));
            }


            return new byte[0][];
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
            throw new NotImplementedException();
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
}
