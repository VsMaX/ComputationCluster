using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;
using Communication_Library;
namespace Task_Manager
{
    public class TSP : TaskSolver
    {       
        public TSP(byte[] problemData) :base(problemData)
        {
            this._problemData = problemData;
        }
        public override byte[][] DivideProblem(int threadCount)
        {
           string stringData = CommunicationModule.ConvertDataToString(_problemData, _problemData.Length);
           string[] split = stringData.Split(' ');
           int ilosc = split.Length / threadCount;
           string[] jak = new string[threadCount];
           byte[][] wynik = new byte[threadCount][];

           for (int i = 0; i < jak.Length; i++)
           {
               jak[i] = split[i];
               wynik[i] = CommunicationModule.ConvertStringToData(jak[i]);
           }
           PartialProblems = wynik;
            return wynik;
        }

        public override event UnhandledExceptionEventHandler ErrorOccured;

        public override void MergeSolution(byte[][] solutions)
        {
            Solution = PartialProblems[0];
        }

        public override string Name
        {
            get { return "TSP"; }
        }

        public override event TaskSolver.ComputationsFinishedEventHandler ProblemDividingFinished;

        public override event TaskSolver.ComputationsFinishedEventHandler ProblemSolvingFinished;

        public override event TaskSolver.ComputationsFinishedEventHandler SolutionsMergingFinished;

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            return partialData;
        }
    }
}
