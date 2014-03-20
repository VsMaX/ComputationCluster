using System;
using System.Collections.Generic;
using Communication_Library;
using Computational_Node;
using Computational_Server;
using Computational_Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    [TestClass]
    public class ComputationClientTests
    {
        private int computationServerPort = 5679;
        private ComputationServer server;
        

        private void StartServer()
        {
            var server = new ComputationServer(computationServerPort);

            server.StartListening();
        }

        private void StopServer()
        {
            server.StopListening();
        }

        [TestInitialize]
        public void TestSetup()
        {
            StartServer();
        }

        [TestCleanup]
        public void TestClean()
        {
            StopServer();
        }

        [TestMethod]
        public void CC_To_CS_Communication_Test()
        {
            StartServer();

            var client = new ComputationClient();
            

            var problemRequest = new SolveRequestMessage();
            
            string ip = "127.0.0.1:5679";
            client.Connect(ip);
            client.SendSolveRequest(problemRequest);
            server.StopListening();

            Assert.AreEqual(server.SolveRequests.Count, 1);
        }

        [TestMethod]
        public void CN_Register_To_CS_Test()
        {
            var computationalNode = new ComputationnalNode();
            
        }
    }
}