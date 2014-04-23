using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private int computationServerPort = 22222;
        private string computationServerIp = "127.0.0.1";
        private ComputationServer server;
        

        private void StartServer()
        {
            server = new ComputationServer(new TimeSpan(0,0,30), null);
            server.StartServer();
        }

        private void StopServer()
        {
            server.StopServer();
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
    }
}