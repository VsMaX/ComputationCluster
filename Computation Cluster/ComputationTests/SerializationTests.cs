using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    [TestClass]
    public class SerializationTests
    {
        [DeploymentItem(@"XMLTestData\SolveRequestMessage.xml", "XMLTestData")]
        [TestMethod]
        public void SolveRequestMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\SolveRequestMessage.xml");
            var serializer = new ComputationSerializer<SolveRequestMessage>();
            var solveRequestMessage = new SolveRequestMessage()
            {
                Data = new byte[]{0,0,25},
                ProblemType = "TSP",
                SolvingTimeout = 15
            };
            var result = serializer.Serialize(solveRequestMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\RegisterMessage.xml", "XMLTestData")]
        [TestMethod]
        public void RegisterMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\RegisterMessage.xml");
            var serializer = new ComputationSerializer<RegisterMessage>();
            var registerMessage = new RegisterMessage()
            {
                Type = RegisterType.ComputationalNode,
                SolvableProblems = new string[]{"ab","ba"},
                ParallelThreads = 15
            };
            var result = serializer.Serialize(registerMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\RegisterResponseMessage.xml", "XMLTestData")]
        [TestMethod]
        public void RegisterResponseMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\RegisterResponseMessage.xml");
            var serializer = new ComputationSerializer<RegisterResponseMessage>();
            var registerResponseMessage = new RegisterResponseMessage()
            {
                Id = 1,
                Timeout = new DateTime(2010, 1, 18),
            };
            var result = serializer.Serialize(registerResponseMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\StatusMessage.xml", "XMLTestData")]
        [TestMethod]
        public void StatusMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\StatusMessage.xml");
            var serializer = new ComputationSerializer<StatusMessage>();

            StatusThread st = new StatusThread(){
                State = StatusThreadState.Idle,
                HowLong = 1,
                ProblemInstanceId = 1,
                ProblemInstanceIdSpecified = true,
                TaskId = 5,
                TaskIdSpecified = true,
                ProblemType = "TSP"
            };

            var statusMessage = new StatusMessage()
            {
                Id = 1,
                Threads = new StatusThread[]{st}
            };
            var result = serializer.Serialize(statusMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\SolveRequestResponseMessage.xml", "XMLTestData")]
        [TestMethod]
        public void SolveRequestResponseMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\SolveRequestResponseMessage.xml");
            var serializer = new ComputationSerializer<SolveRequestResponseMessage>();
            var solveRequestResponseMessage = new SolveRequestResponseMessage()
            {
                Id = 1
            };
            var result = serializer.Serialize(solveRequestResponseMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\DivideProblemMessage.xml", "XMLTestData")]
        [TestMethod]
        public void DivideProblemMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\DivideProblemMessage.xml");
            var serializer = new ComputationSerializer<DivideProblemMessage>();
            var divideProblemMessage = new DivideProblemMessage()
            {
                ProblemType="TSP",
                Id = 1,
                Data = new byte[] { 0, 0, 25 },
                ComputationalNodes = 5
            };
            var result = serializer.Serialize(divideProblemMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\SolutionRequestMessage.xml", "XMLTestData")]
        [TestMethod]
        public void SolutionRequestMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\SolutionRequestMessage.xml");
            var serializer = new ComputationSerializer<SolutionRequestMessage>();
            var solutionRequestMessage = new SolutionRequestMessage()
            {
                Id = 1
            };
            var result = serializer.Serialize(solutionRequestMessage);
            Assert.AreEqual(result, testData);
        }

        [DeploymentItem(@"XMLTestData\PartialProblemsMessage.xml", "XMLTestData")]
        [TestMethod]
        public void PartialProblemsMessageSerialization()
        {
            string testData = System.IO.File.ReadAllText(@"XMLTestData\PartialProblemsMessage.xml");
            var serializer = new ComputationSerializer<PartialProblemsMessage>();

            SolvePartialProblemsPartialProblem pp = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 20,
                Data = new byte[] { 0, 0, 10 }
            };

            var partialProblemsMessage = new PartialProblemsMessage()
            {
                ProblemType="TSP",
                Id = 1,
                CommonData = new byte[] { 0, 0, 25 },
                SolvingTimeout = 50,
                SolvingTimeoutSpecified = true,
                PartialProblems = new SolvePartialProblemsPartialProblem[]{pp}
            };
            var result = serializer.Serialize(partialProblemsMessage);
            Assert.AreEqual(result, testData);
        }
    }
}