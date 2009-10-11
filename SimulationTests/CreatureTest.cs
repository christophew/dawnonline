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

            critter.Statistics.WalkingDistance = 10;

            environment.AddCreature(critter, new Coordinate { X = 10, Y = 20 }, 0);

            Assert.AreEqual(10, critter.Place.Position.X);
            Assert.AreEqual(20, critter.Place.Position.Y);

            critter.WalkForward();

            Assert.AreEqual(20, critter.Place.Position.X);
            Assert.AreEqual(20, critter.Place.Position.Y);
        }

        [TestMethod]
        [Ignore]
        public void Test_Attack()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = critter1.Statistics.MeleeRange / 2, Y = 0 }, 0);

            Assert.AreEqual(critter2, critter1.Attack());
        }

        [TestMethod]
        [Ignore]
        public void Test_Attack2()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = 0, Y = critter1.Statistics.MeleeRange - 1 }, 0);

            Assert.AreEqual(critter2, critter1.Attack());
        }

        [TestMethod]
        [Ignore]
        public void Test_NoAttack()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = critter1.Statistics.MeleeRange + 1, Y = 0 }, 0);

            Assert.IsNull(critter1.Attack());
        }
    }
}
