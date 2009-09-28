using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DawnOnline.Simulation.UnitTests
{
    [TestFixture]
    public class EnvironmentTest
    {
        [Test]
        public void Test_Add()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var myCritter = SimulationFactory.CreatePredator();

            environment.AddCreature(myCritter, new Coordinate {X=10, Y=20}, 0);

            Assert.AreEqual(1, environment.GetCreatures().Count);
            Assert.AreEqual(myCritter, environment.GetCreatures()[0]);

            Assert.AreEqual(1, environment.GetCreatures().Count);
            var retrievedPlacement = environment.GetCreatures()[0].Place;
            Assert.AreEqual(10, retrievedPlacement.Position.X);
            Assert.AreEqual(20, retrievedPlacement.Position.Y);
        }

        [Test]
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
