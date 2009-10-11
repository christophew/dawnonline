using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    /// <summary>
    /// Summary description for EyeTest
    /// </summary>
    [TestClass]
    public class EyeTest
    {
        public EyeTest()
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
        public void TestSees()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = 10, Y = 10 }, 0);

            var eye = new Eye(critter1);
            eye.Angle = 0.0;
            eye.VisionDistance = 50;
            eye.VisionAngle = MathTools.ConvertToRadials(45);
            Assert.IsTrue(eye.Sees(critter2));

            // Test VisionAngle
            eye.VisionAngle = MathTools.ConvertToRadials(44);
            Assert.IsFalse(eye.Sees(critter2));

            // Test Eye.Angle
            eye.Angle = MathTools.ConvertToRadials(1.0);
            Assert.IsTrue(eye.Sees(critter2));

            // Test Critter.Angle
            eye.Angle = MathTools.ConvertToRadials(-1);
            critter1.Statistics.TurningAngle = MathTools.ConvertToRadials(1.0);
            Assert.IsFalse(eye.Sees(critter2));
            critter1.TurnRight();
            Assert.IsFalse(eye.Sees(critter2));
            critter1.TurnRight();
            Assert.IsTrue(eye.Sees(critter2));

            // Test VisionDistance
            eye.VisionDistance = Math.Sqrt(200);
            Assert.IsTrue(eye.Sees(critter2));
            eye.VisionDistance = Math.Sqrt(200) - 1;
            Assert.IsFalse(eye.Sees(critter2));
        }
    }
}
