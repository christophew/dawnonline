using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.UnitTests
{
    [TestFixture]
    public class EyeTest
    {
        [Test]
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
