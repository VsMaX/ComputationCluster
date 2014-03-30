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
        private int computationServerPort = 8001;
        private ComputationServer server;
        

        private void StartServer()
        {
            server = new ComputationServer(computationServerPort);
            server.StartListening();
        }

        private void StopServer()
        {
            //server.StopListening();
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
            //ARRANGE
            var client = new ComputationClient();

            var problemRequest = new SolveRequestMessage();
            string ip = "192.168.1.24";

            //ACT
            client.Connect(ip);
            client.SendSolveRequest(problemRequest);

            //ASSERT
            //uwaga tu moze byc deadlock
            Assert.AreEqual(server.GetUnfinishedTasks().Count, 1);
        }

        //[TestMethod]
        //public void CN_Register_To_CS_Test()
        //{
        //    var computationalNode = new ComputationnalNode();

        //    computationalNode.RegisterAtServer();

        //    Assert.AreEqual(server.ActiveNodes.Count, 1);
        //}

        [TestMethod]
        public void CS_Send_Problem_To_Task_Manager()
        {
            
        }

        [TestMethod]
        public void CN_Send_Solved_Subtask_To_CS()
        {
            
        }

        [TestMethod]
        public void CS_Divide_Subtasks_Among_CNs()
        {
            
        }

        [TestMethod]
        public void CS_Send_Solution_To_CC()
        {
            
        }
    }
}