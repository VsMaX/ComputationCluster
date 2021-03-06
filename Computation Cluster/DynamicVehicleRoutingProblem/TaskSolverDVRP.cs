﻿using System;
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
        public long[,] comb;

        public TaskSolverDVRP(byte[] problemData)
            : base(problemData)
        {
            if (problemData.Length > 0 && problemData != null)
            {
                this._problemData = problemData;
                this.Dvrp = DVRPHelper.Parse(ConvertDataToString(problemData, problemData.Length));
                this.comb = DVRPHelper.GetAllCombination(Dvrp.ClientID.Length + 1);
            }
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            this.State = TaskSolverState.Dividing;

            var result = new byte[threadCount][];

            var subsets = CreateSubsets(Dvrp.ClientID);//.ToArray();

            subsets.Sort(
                   delegate(int[] tab1, int[] tab2)
                   {
                       if (tab1.Length < tab2.Length)
                           return -1;
                       else if (tab2.Length < tab1.Length)
                           return 1;
                       else
                       {
                           for (int i = 0; i < tab1.Length; i++)
                           {
                               if (tab1[i] < tab2[i])
                                   return -1;
                               else if (tab2[i] < tab1[i])
                                   return 1;
                           }
                           return 0;
                       }
                   }
               );


            Trace.WriteLine(subsets.Count().ToString());
            int size = subsets.Count() / (threadCount);
            Trace.WriteLine(size.ToString());
            int[][] res = new int[threadCount][];
            int indeks = 0;
            int wpisane = 0;
            for (int i = 1; i <= threadCount; i++)
            {
                int[][] tab = new int[size][];
                if (i == threadCount) // ostatni podzial
                {
                    tab = new int[subsets.Count() - size * (i-1)][];
                    size = tab.Count();
                }

                int ind = 0;
                for (int j = 0; j < size; j++, ind++)
                {
                    
                    tab[ind] = new int[subsets[j+wpisane].Length];

                    for (int k = 0; k < subsets[j+wpisane].Length; k++)
                    {
                        tab[ind] = new int[subsets[j+wpisane].Length];
                        tab[ind] = subsets[j+wpisane];
                    }
                }
                wpisane += size;
                result[i - 1] = ConvertStringToData(Client.ClientsToString(tab, indeks));
                indeks++;
            }

            return result;
        }
        public static List<T[]> CreateSubsets<T>(T[] originalArray)
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

            DVRPPartialSolution[] ps = new DVRPPartialSolution[solutions.Length];

            for (int i = 0; i < solutions.Length; i++)
            {
                DVRPPartialSolution sol = DVRPPartialSolution.Parse(ConvertDataToString(solutions[i], solutions[i].Length), Dvrp);
                ps[sol.NodeNumber] = sol;
            }

            double bestLen = Double.MaxValue;
            List<int> bestsolNode = new List<int>();
            List<int> bestsolInd = new List<int>();



            foreach (var part in GetAllPartitions<int>(Dvrp.ClientID))
            {
                double len = 0;
                int node = 0, ind = 0;

                List<int> pomsolNode = new List<int>();
                List<int> pomsolInd = new List<int>();

                for (int i = 0; i < part.Length; i++)
                {

                    int index = DVRPHelper.GetIndex(part[i], this.comb,Dvrp.ClientID.Length);
                    DVRPHelper.GetNodeAndInd(index, ps, out node, out ind);

                    if (ps[node].PartialPathLen[ind] < 0) // sciezka nie istnieje
                    {
                        len = Double.MaxValue;
                        break;
                    }
                    else
                    {
                        len += ps[node].PartialPathLen[ind];
                        pomsolNode.Add(node);
                        pomsolInd.Add(ind);

                        if (len > bestLen)
                            break;
                    }
                }
                if (len < bestLen)
                {
                    bestLen = len;
                    bestsolNode = new List<int>(pomsolNode);
                    bestsolInd = new List<int>(pomsolInd);
                }
            }
            //DVRPSolution solution  = new DVRPSolution(0, ps[bestsolNode].PartialPathLen[bestsolInd], ps[bestsolNode].PartialPaths[bestsolInd], ps[bestsolNode].PartialPathsArrivalsTimes[bestsolInd]);
            DVRPPartialSolution solution = new DVRPPartialSolution(bestsolNode, bestsolInd, ps, bestLen);
            string StringSol = DVRPPartialSolution.SolutionToString(solution);
            this.Solution = ConvertStringToData(StringSol);
        }
            //foreach (var part in GetAllPartitions<int>(Dvrp.ClientID))
            //{
            //    double len = 0;
            //    for (int i = 0; i < part.Length; i++)
            //    {
            //        for (int j = 0; j < solutions.Length; j++)
            //        {
            //            for (int k=0; k< ps[j].PartialClientID.Length; k++)
            //            if (DVRPHelper.CompareArrays(ps[j].PartialClientID[k].ToArray(), part[i]))
            //            {
            //                if (ps[j].PartialPathLen[k] == -1)
            //                    len = Double.MaxValue;
            //                else
            //                    len += ps[j].PartialPathLen[k];
            //            }
            //        }
            //    }
            //    if (len < bestLen)
            //    {
            //        bestLen = len;
            //    }
            //}

        

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
            DVRPPartialDataToSolve pd2s = new DVRPPartialDataToSolve();

            pd2s = DVRPPartialDataToSolve.ParsePartialProblemData(partialData);

            DVRPPartialSolution solution;
            string solString = "SOL:" + pd2s.partial.Length.ToString() + ":" + pd2s.NodeNumber + "\n";
            for (int set = 0; set < pd2s.partial.Length; set++)
            {
                DVRPPathFinder pathFinder = new DVRPPathFinder(pd2s.partial[set], this.Dvrp);
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
                solString += DVRPPartialSolution.ArrayToString(pd2s.partial[set]);
            }
            return ConvertStringToData(solString);
        }

        public static string ConvertDataToString(byte[] buffer, int bytesRead)
        {
            if (bytesRead > 0)
            {
                return Encoding.UTF8.GetString(buffer, 0,
                        bytesRead);
                //message = message.Replace("\0", String.Empty);
                //message = message.Trim();
                //return message;
            }
            return String.Empty;
        }

        public static byte[] ConvertStringToData(string response)
        {
            return Encoding.UTF8.GetBytes(response);
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
