using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace SimulationTests
{
    [TestClass]
    public class EyeTest
    {
        [TestInitialize]
        public void Setup()
        {
            if (Globals.GetInstanceId() != 0)
                Globals.SetInstanceId(0);
        }

        [TestMethod]
        public void CreatureAngle0()
        {
            var environment = DawnOnline.Simulation.Environment.GetWorld();

            var creature = new CreatureOnClient(1);
            environment.AddCreature(creature, Vector2.Zero, 0);

            var eye = SensorBuilder.CreateEye(creature, 0, Math.PI/4.0, 100);

            var creatureToSee = new CreatureOnClient(1);
            environment.AddCreature(creatureToSee, new Vector2(0, 0), 0);
            var creatureList = new List<IEntity>() {creatureToSee};

            // Test on x-axis
            creatureToSee.Place.OffsetPosition(new Vector2(50, 0), 0);
            Assert.IsTrue(eye.WeightedDistanceToFirstVisible(creatureList) > 0);

            creatureToSee.Place.OffsetPosition(new Vector2(100, 0), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);

            // Test on y-axis
            creatureToSee.Place.OffsetPosition(new Vector2(0, 50), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
            creatureToSee.Place.OffsetPosition(new Vector2(0, -50), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);

            // Test on 45° - something
            creatureToSee.Place.OffsetPosition(new Vector2(50, 49), 0);
            Assert.IsTrue(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
            creatureToSee.Place.OffsetPosition(new Vector2(50, -49), 0);
            Assert.IsTrue(eye.WeightedDistanceToFirstVisible(creatureList) > 0);

            // Test on 45° + something
            creatureToSee.Place.OffsetPosition(new Vector2(50, 51), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
            creatureToSee.Place.OffsetPosition(new Vector2(50, -51), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
        }      
  
        [TestMethod]
        public void CreatureAngle45()
        {
            var environment = DawnOnline.Simulation.Environment.GetWorld();

            var creature = new CreatureOnClient(1);
            environment.AddCreature(creature, Vector2.Zero, Math.PI / 4.0);

            var eye = SensorBuilder.CreateEye(creature, 0, Math.PI/4.0, 100);

            var creatureToSee = new CreatureOnClient(1);
            environment.AddCreature(creatureToSee, new Vector2(0, 0), 0);
            var creatureList = new List<IEntity>() { creatureToSee };

            // Test on 45° - something
            creatureToSee.Place.OffsetPosition(new Vector2(50, 1), 0);
            Assert.IsTrue(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
            creatureToSee.Place.OffsetPosition(new Vector2(1, 50), 0);
            Assert.IsTrue(eye.WeightedDistanceToFirstVisible(creatureList) > 0);

            // Test on 45° + something
            creatureToSee.Place.OffsetPosition(new Vector2(50, -1), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
            creatureToSee.Place.OffsetPosition(new Vector2(-1, -50), 0);
            Assert.IsFalse(eye.WeightedDistanceToFirstVisible(creatureList) > 0);
        }
    }
}
