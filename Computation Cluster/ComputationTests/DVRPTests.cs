using System;
using System.Text;
using System.Collections.Generic;
using DynamicVehicleRoutingProblem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    /// <summary>
    /// Summary description for DVRPTests
    /// </summary>
    [TestClass]
    public class DVRPTests
    {
        public DVRPTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [DeploymentItem(@"DVRPTestData\okul12D.vrp", "DVRPTestData")]
        [TestMethod]
        public void DVRPParseTest1()
        {
            string testData = System.IO.File.ReadAllText(@"DVRPTestData\okul12D.vrp");
            DVRP result = DVRP.Parse(testData);
            
            DVRP model = new DVRP();
            model.FormatVersionNumber="okul12a";
            model.Comment = "Best known objective: 976.27TIMESTEP: 7";
            model.Name = "okul12a";
            model.NumDepots= 1;
            model.NumCapacities = 1;
            model.NumVistis = 12;
            model.NumLocations = 13;
            model.NumVehicles = 12;
            model.Capacities = 100;
            
            model.Depots =new Depot[model.NumDepots];
            model.Depots[0] = new Depot();
            model.Depots[0].depotID = 0;
            
            model.Clients = new Client[model.NumVistis];
            for (int i = 0; i < model.NumVistis; i++)
            {
                model.Clients[i] = new Client(); 
            }
            model.Clients[0].visitID = 1;
            model.Clients[0].size = -48;
            model.Clients[0].locationID = 1;
            model.Clients[1].visitID = 2;
            model.Clients[1].size = -20;
            model.Clients[1].locationID = 2;
            model.Clients[2].visitID = 3;
            model.Clients[2].size = -45;
            model.Clients[2].locationID = 3;
            model.Clients[3].visitID = 4;
            model.Clients[3].size = -19;
            model.Clients[3].locationID = 4;
            model.Clients[4].visitID = 5;
            model.Clients[4].size = -32;
            model.Clients[4].locationID = 5;
            model.Clients[5].visitID = 6;
            model.Clients[5].size = -42;
            model.Clients[5].locationID = 6;
            model.Clients[6].visitID = 7;
            model.Clients[6].size = -19;
            model.Clients[6].locationID = 7;
            model.Clients[7].visitID = 8;
            model.Clients[7].size = -35;
            model.Clients[7].locationID = 8;
            model.Clients[8].visitID = 9;
            model.Clients[8].size = -30;
            model.Clients[8].locationID = 9;
            model.Clients[9].visitID = 10;
            model.Clients[9].size = -26;
            model.Clients[9].locationID = 10;
            model.Clients[10].visitID = 11;
            model.Clients[10].size = -42;
            model.Clients[10].locationID = 11;
            model.Clients[11].visitID = 12;
            model.Clients[11].size = -27;
            model.Clients[11].locationID = 12;

            model.Locations = new Location[model.NumLocations];
            for (int i = 0; i < model.NumLocations; i++)
            {
                model.Locations[i] = new Location();
            }
            model.Locations[0].locationID = 0;
            model.Locations[0].x = 0;
            model.Locations[0].y = 0;
            model.Locations[1].locationID = 1;
            model.Locations[1].x = -55;
            model.Locations[1].y = -26;
            model.Locations[2].locationID = 2;
            model.Locations[2].x = -24;
            model.Locations[2].y = 38;
            model.Locations[3].locationID = 3;
            model.Locations[3].x = -99;
            model.Locations[3].y = -29;
            model.Locations[4].locationID = 4;
            model.Locations[4].x = -42;
            model.Locations[4].y = 30;
            model.Locations[5].locationID = 5;
            model.Locations[5].x = 59;
            model.Locations[5].y = 66;
            model.Locations[6].locationID = 6;
            model.Locations[6].x = 55;
            model.Locations[6].y = -35;
            model.Locations[7].locationID = 7;
            model.Locations[7].x = -42;
            model.Locations[7].y = 3;
            model.Locations[8].locationID = 8;
            model.Locations[8].x = 95;
            model.Locations[8].y = 13;
            model.Locations[9].locationID = 9;
            model.Locations[9].x = 71;
            model.Locations[9].y = -90;
            model.Locations[10].locationID = 10;
            model.Locations[10].x = 38;
            model.Locations[10].y = 32;
            model.Locations[11].locationID = 11;
            model.Locations[11].x = 67;
            model.Locations[11].y = -22;
            model.Locations[12].locationID = 12;
            model.Locations[12].x = 58;
            model.Locations[12].y = -97;

            //DEPOT_LOCATION_SECTION
            model.Depots[0].locationID = 0;

            foreach(var v in model.Clients)
                v.unld = 20;

            //DEPOT_TIME_WINDOW_SECTION
            model.Depots[0].start=0;
            model.Depots[0].end = 640;

            //TIME_AVAIL_SECTION
            model.Clients[0].time=616;
            model.Clients[1].time=91;
            model.Clients[2].time=240;
            model.Clients[3].time=356;
            model.Clients[4].time=528;
            model.Clients[5].time=459;
            model.Clients[6].time=433;
            model.Clients[7].time=513;
            model.Clients[8].time=444;
            model.Clients[9].time=44;
            model.Clients[10].time=318;
            model.Clients[11].time=20;

            Assert.AreEqual<DVRP>(result, model);
        }
    }
}
