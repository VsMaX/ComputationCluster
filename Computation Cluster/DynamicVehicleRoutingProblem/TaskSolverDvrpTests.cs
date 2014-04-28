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
        [DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        [DeploymentItem(@"DVRPTestData\AllTxtFiles.txt", "DVRPTestData")]
        [TestMethod]
        [Timeout(120000)]
        public void DivideProblemTest()
        {
            string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
            byte[] problemData = CommunicationModule.ConvertStringToData(testData);
            TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
            byte[][] result = taskSolver.DivideProblem(10);
            string fistNodeText = System.IO.File.ReadAllText(@"DVRPTestData\AllTxtFiles.txt");
            int[][][] firstNodeTab = DVRP.ParseData(CommunicationModule.ConvertStringToData(fistNodeText));
            int[][][] firstNodeTest = DVRP.ParseData(result[0]);

            bool ok = true;
            string msg = "";
            if (firstNodeTab.Length == firstNodeTest.Length)
            {
                for (int i = 0; i < firstNodeTab.Length; i++)
                {
                    if (firstNodeTab[i].Length == firstNodeTest[i].Length)
                    {
                        for (int j = 0; j < firstNodeTab[i].Length; j++)
                        {
                            if (firstNodeTab[i][j].Length == firstNodeTest[i][j].Length)
                            {
                                for (int k = 0; k < firstNodeTab[i][j].Length; k++)
                                {
                                    if (firstNodeTab[i][j][k] != firstNodeTest[i][j][k])
                                    {
                                        msg = "firstNodeTab[i][j][k] != firstNodeTest[i][j][k]";
                                        ok = false;
                                        break;
                                    }
                                }
                                if (!ok)
                                    break;
                            }
                            else
                            {
                                msg = "firstNodeTab[i][j].Length != firstNodeTest[i][j].Length";
                                ok = false;
                                break;
                            }
                        }
                        if (!ok)
                            break;
                    }
                    else
                    {
                        msg = "firstNodeTab[i].Length == firstNodeTest.Length";
                        ok = false;
                        break;
                    }
                }
            }
            else
            {
                msg = "firstNodeTab.Length != firstNodeTest.Length";
                ok = false;
            }

            Assert.IsTrue(ok,msg);
        }

        [DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        [DeploymentItem(@"DVRPTestData\AllTxtFiles.txt", "DVRPTestData")]
        [TestMethod]
        [Timeout(120000)]
        public void SolveProblemTest()
        {
            var testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
            DVRP result = DVRP.Parse(testData);
 
        }

    }
}
