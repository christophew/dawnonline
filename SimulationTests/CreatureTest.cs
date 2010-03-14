using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    /// <summary>
    /// Summary description for CreatureTest
    /// </summary>
    [TestClass]
    public class CreatureTest
    {
        public CreatureTest()
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
        public void Test_Walk()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter = new Creature(15);

            critter.CharacterSheet.WalkingDistance = 10;

            environment.AddCreature(critter, new Coordinate { X = 10, Y = 20 }, 0);

            Assert.AreEqual(10, critter.Place.Position.X);
            Assert.AreEqual(20, critter.Place.Position.Y);

            critter.WalkForward();

            Assert.AreEqual(20, critter.Place.Position.X);
            Assert.AreEqual(20, critter.Place.Position.Y);
        }
    }
}
