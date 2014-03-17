﻿using System;
using Communication_Library;
using Computational_Server;
using Copmutational_Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    [TestClass]
    public class ComputationClientTests
    {
        private int computationServerPort = 5679;

        [TestMethod]
        public void CC_To_CS_Communication_Test()
        {
            var client = new ComputationClient();
            var server = new ComputationServer(computationServerPort);

            server.StartListening();

            var problemRequest = new ProblemRequest();
            string ip = "127.0.0.1:5679";
            client.Connect(ip);
            server.StopListening();

            //Assert.AreEqual(server.ProblemQueue.Count, 1);
        }
    }


}
