using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    [TestClass]
    public class ComputationClientTests
    {
        private string computationServerIp = "127.0.0.1";
        private string computationServerPort = "5679";

        [TestMethod]
        public void SendProblemToCSTest()
        {
            //var client = new ComputationClient();
            //var server = new ComputationServer();

            //server.StartListening(computationServerIp, computationServerPort);

            //var problemRequest = new ProblemRequest();

            //client.SendProblemRequest(problemRequest);

            //server.StopListening();

            //Assert.AreEqual(server.ProblemQueue.Count(), 1);

        }
    }
}
