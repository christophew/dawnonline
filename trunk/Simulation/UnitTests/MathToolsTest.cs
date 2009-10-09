using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.UnitTests
{
    [TestFixture]
    public class MathToolsTest
    {
        [Test]
        public void TestDistance2()
        {
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 0 }, new Coordinate { X = 10, Y = 0 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 0 }, new Coordinate { X = 0, Y = 10 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 10, Y = 0 }, new Coordinate { X = 0, Y = 0 }));
            Assert.AreEqual(100, MathTools.GetDistance2(new Coordinate { X = 0, Y = 10 }, new Coordinate { X = 0, Y = 0 }));
        }

        [Test]
        public void TestConvertToDegrees()
        {
            Assert.AreEqual(0, MathTools.ConvertToRadials(0));
            Assert.AreEqual(Math.PI, MathTools.ConvertToRadials(180));
            Assert.AreEqual(Math.PI / 2.0, MathTools.ConvertToRadials(90));
            Assert.AreEqual(Math.PI / 4.0, MathTools.ConvertToRadials(45));
        }

        [Test]
        public void TestAngle()
        {
            Assert.AreEqual(MathTools.ConvertToRadials(45), MathTools.GetAngle(0, 0, 10, 10));
            Assert.AreEqual(MathTools.ConvertToRadials(90 + 45), MathTools.GetAngle(0, 0, -10, 10));
            Assert.AreEqual(MathTools.ConvertToRadials(180 + 45), MathTools.GetAngle(0, 0, -10, -10));
            Assert.AreEqual(MathTools.ConvertToRadials(270 + 45), MathTools.GetAngle(0, 0, 10, -10));
        }

        [Test]
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
