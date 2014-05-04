using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicVehicleRoutingProblem;
using Communication_Library;
using System.Threading;

namespace ComputationTests
{
    [TestClass]
    public class TaskSolverDvrpTests
    {
        //[DeploymentItem(@"DVRPTestData\okul13D.vrp", "DVRPTestData")]
        //[DeploymentItem(@"DVRPTestData\AllTxtFiles.txt", "DVRPTestData")]
        //[TestMethod]
        //[Timeout(120000)]
        //public void DivideProblemTest()
        //{
        //    string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul13D.vrp");
        //    byte[] problemData = CommunicationModule.ConvertStringToData(testData);
        //    TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
        //    byte[][] result = taskSolver.DivideProblem(10);
        //    string fistNodeText = System.IO.File.ReadAllText(@"DVRPTestData\AllTxtFiles.txt");
        //    //int[][][] firstNodeTab = DVRPHelper.ParsePartialProblemData(CommunicationModule.ConvertStringToData(fistNodeText));
        //    //int[][][] firstNodeTest = DVRPHelper.ParsePartialProblemData(result[0]);

        //    bool ok = true;
        //    string msg = "";
        //    if (firstNodeTab.Length == firstNodeTest.Length)
        //    {
        //        for (int i = 0; i < firstNodeTab.Length; i++)
        //        {
        //            if (firstNodeTab[i].Length == firstNodeTest[i].Length)
        //            {
        //                for (int j = 0; j < firstNodeTab[i].Length; j++)
        //                {
        //                    if (firstNodeTab[i][j].Length == firstNodeTest[i][j].Length)
        //                    {
        //                        for (int k = 0; k < firstNodeTab[i][j].Length; k++)
        //                        {
        //                            if (firstNodeTab[i][j][k] != firstNodeTest[i][j][k])
        //                            {
        //                                msg = "firstNodeTab[i][j][k] != firstNodeTest[i][j][k]";
        //                                ok = false;
        //                                break;
        //                            }
        //                        }
        //                        if (!ok)
        //                            break;
        //                    }
        //                    else
        //                    {
        //                        msg = "firstNodeTab[i][j].Length != firstNodeTest[i][j].Length";
        //                        ok = false;
        //                        break;
        //                    }
        //                }
        //                if (!ok)
        //                    break;
        //            }
        //            else
        //            {
        //                msg = "firstNodeTab[i].Length == firstNodeTest.Length";
        //                ok = false;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        msg = "firstNodeTab.Length != firstNodeTest.Length";
        //        ok = false;
        //    }

        //    Assert.IsTrue(ok,msg);
        //}

        //[DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        //[TestMethod]
        //[Timeout(24000000)]
        //public void SolveProblemTest12D()
        //{
        //    string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
        //    byte[] problemData = CommunicationModule.ConvertStringToData(testData);
        //    TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
        //    byte[][] division = taskSolver.DivideProblem(10);
        //    byte[][] solutions = new byte[10][];
        //    for (int i = 0; i <=9; i++)
        //    {
        //        int[][][] partialData = DVRPHelper.ParsePartialProblemData(division[i]);
        //        solutions[i] = taskSolver.Solve(division[i], new TimeSpan());
        //    }
        //    taskSolver.MergeSolution(solutions);
        //    DVRPSolution finalSol = DVRPSolution.Parse(CommunicationModule.ConvertDataToString(taskSolver.Solution, taskSolver.Solution.Length), taskSolver.Dvrp);

        //    Assert.IsTrue(Math.Abs(finalSol.pathLen - 976) < 1);
        //}
        [DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        [TestMethod]
        [Timeout(24000000)]
        public void SolveProblemTest13D()
        {
            string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
            byte[] problemData = CommunicationModule.ConvertStringToData(testData);
            TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
            byte[][] division = taskSolver.DivideProblem(7);
            byte[][] solutions = new byte[7][];
            for (int i = 0; i <= 6; i++)
            {
                int[][] partialData = DVRPHelper.ParsePartialProblemData(division[i]);
                solutions[i] = taskSolver.Solve(division[i], new TimeSpan());
            }
            taskSolver.MergeSolution(solutions);
            DVRPSolution finalSol = DVRPSolution.Parse(CommunicationModule.ConvertDataToString(taskSolver.Solution, taskSolver.Solution.Length), taskSolver.Dvrp);

            Assert.IsTrue(Math.Abs(finalSol.pathLen - 1154) < 1);
        }
        [DeploymentItem(@"DVRPTestData\okul14D.vrp", "DVRPTestData")]
        [TestMethod]
        [Timeout(24000000)]
        public void SolveProblemTest14D()
        {
            string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul14D.vrp");
            byte[] problemData = CommunicationModule.ConvertStringToData(testData);
            TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
            byte[][] division = taskSolver.DivideProblem(8);
            byte[][] solutions = new byte[8][];
            for (int i = 0; i <= 7; i++)
            {
                //int[][][] partialData = DVRPHelper.ParsePartialProblemData(division[i]);
                solutions[i] = taskSolver.Solve(division[i], new TimeSpan());
            }
            taskSolver.MergeSolution(solutions);
            DVRPSolution finalSol = DVRPSolution.Parse(CommunicationModule.ConvertDataToString(taskSolver.Solution, taskSolver.Solution.Length), taskSolver.Dvrp);

            Assert.IsTrue(Math.Abs(finalSol.pathLen - 948) < 1);
        }
    }
}
