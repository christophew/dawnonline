using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    /// <summary>
    /// Summary description for MathToolsTest
    /// </summary>
    [TestClass]
    public class MathToolsTest
    {
        public MathToolsTest()
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
        public void TestDistance2()
        {
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 0 }, new Coordinate { X = 10, Y = 0 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 0 }, new Coordinate { X = 0, Y = 10 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 10, Y = 0 }, new Coordinate { X = 0, Y = 0 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 10 }, new Coordinate { X = 0, Y = 0 }));
        }

        [TestMethod]
        public void TestConvertToDegrees()
        {
            Assert.AreEqual(0, MathTools.ConvertToRadials(0));
            Assert.AreEqual(Math.PI, MathTools.ConvertToRadials(180));
            Assert.AreEqual(Math.PI / 2.0, MathTools.ConvertToRadials(90));
            Assert.AreEqual(Math.PI / 4.0, MathTools.ConvertToRadials(45));
        }

        [TestMethod]
        public void TestAngle()
        {
            Assert.AreEqual(MathTools.ConvertToRadials(45), MathTools.GetAngle(0, 0, 10, 10));
            Assert.AreEqual(MathTools.ConvertToRadials(90 + 45), MathTools.GetAngle(0, 0, -10, 10));
            Assert.AreEqual(MathTools.ConvertToRadials(180 + 45), MathTools.GetAngle(0, 0, -10, -10));
            Assert.AreEqual(MathTools.ConvertToRadials(270 + 45), MathTools.GetAngle(0, 0, 10, -10));
        }

        [TestMethod]
        public void TestOffsetCoordinate()
        {
            var origin = new Coordinate(0, 0);
            Assert.AreEqual(10, MathTools.OffsetCoordinate(origin, 0, 10).X);
            Assert.AreEqual(-10, MathTools.OffsetCoordinate(origin, Math.PI, 10).X);
            Assert.AreEqual(10, MathTools.OffsetCoordinate(origin, Math.PI / 2.0, 10).Y);
            Assert.AreEqual(-10, MathTools.OffsetCoordinate(origin, 3.0 * Math.PI / 2.0, 10).Y);
        }
    }
}
