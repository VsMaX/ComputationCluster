using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace DynamicVehicleRoutingProblem
{
    public class TaskSolver<T> : TaskSolver where T : DVRP
    {


        public TaskSolver(byte[] problemData)
            : base(problemData)
        {
            this._problemData = problemData;

        }
        public override byte[][] DivideProblem(int threadCount)
        {
            return null;
        }

        public override event UnhandledExceptionEventHandler ErrorOccured;

        public override void MergeSolution(byte[][] solutions)
        {
            Solution = PartialProblems[0];
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
            return partialData;
        }
    }
}
