using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicVehicleRoutingProblem;
using Communication_Library;

namespace ComputationTests
{
    [TestClass]
    public class TaskSolverDvrpTests
    {
        [DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        [TestMethod]
        [Timeout(60000)]
        public void DivideProblemTest()
        {
            string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
            byte[] problemData = CommunicationModule.ConvertStringToData(testData);
            TaskSolverDVRP taskSolver = new TaskSolverDVRP(problemData);
            taskSolver.DivideProblem(10);
        }
    }
}