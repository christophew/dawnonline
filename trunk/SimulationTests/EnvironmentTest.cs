﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    /// <summary>
    /// Summary description for EnvironmentTest
    /// </summary>
    [TestClass]
    public class EnvironmentTest
    {
        public EnvironmentTest()
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

        [TestMethod]
        public void Test_Add()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var myCritter = SimulationFactory.CreatePredator();

            environment.AddCreature(myCritter, new Coordinate { X = 10, Y = 20 }, 0);

            Assert.AreEqual(1, environment.GetCreatures().Count);
            Assert.AreEqual(myCritter, environment.GetCreatures()[0]);

            Assert.AreEqual(1, environment.GetCreatures().Count);
            var retrievedPlacement = environment.GetCreatures()[0].Place;
            Assert.AreEqual(10, retrievedPlacement.Position.X);
            Assert.AreEqual(20, retrievedPlacement.Position.Y);
        }

        [TestMethod]
        public void Test_Kill()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var myCritter = SimulationFactory.CreatePredator();
            environment.AddCreature(myCritter, new Coordinate { X = 10, Y = 20 }, 0);

            Assert.IsTrue(myCritter.Alive);
            environment.KillCreature(myCritter);
            Assert.IsFalse(myCritter.Alive);
        }
    }
}