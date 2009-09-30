using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DawnOnline.Simulation.UnitTests
{
    [TestFixture]
    public class CreatureTest
    {
        [Test]
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

        [Test]
        public void Test_Attack()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = critter1.Place.Form.Radius / 2, Y = 0 }, 0);

            Assert.AreEqual(critter2, critter1.Attack());
        }

        [Test]
        public void Test_Attack2()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = 0, Y = critter1.Place.Form.Radius - 1 }, 0);

            Assert.AreEqual(critter2, critter1.Attack());
        }

        [Test]
        public void Test_NoAttack()
        {
            var environment = SimulationFactory.CreateEnvironment();
            var critter1 = new Creature(15);
            environment.AddCreature(critter1, new Coordinate { X = 0, Y = 0 }, 0);
            var critter2 = new Creature(15);
            environment.AddCreature(critter2, new Coordinate { X = critter1.Place.Form.Radius + 1, Y = 0 }, 0);

            Assert.IsNull(critter1.Attack());
        }

        [Test]
        public void Test_See()
        {
            
        }
    }
}
