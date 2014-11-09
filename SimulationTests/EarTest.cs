using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace SimulationTests
{
    [TestClass]
    public class EarTest
    {
        [TestMethod]
        public void VolumeTest()
        {
            var environment = DawnOnline.Simulation.Environment.GetWorld();

            var creature = new CreatureOnClient(1);
            environment.AddCreature(creature, Vector2.Zero, 0);
            var ear = new Ear(creature, Vector2.Zero);

            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));

            // No volume
            var soundWithNoVolume = new Sound(new Vector2(10, 0), Sound.SoundTypeEnum.A, creature.FamilyId, 0, 1);
            environment.AddSound(soundWithNoVolume);

            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));

            // I hear you
            var normalSound = new Sound(new Vector2(10, 0), Sound.SoundTypeEnum.A, creature.FamilyId, 10, 1);
            environment.AddSound(normalSound);

            Assert.AreNotEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));

            // I hear you better
            var harderSound = new Sound(new Vector2(10, 0), Sound.SoundTypeEnum.B, creature.FamilyId, 20, 1);
            environment.AddSound(harderSound);

            Assert.AreNotEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreNotEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));
            Assert.IsTrue(ear.HearFamily(Sound.SoundTypeEnum.A) < ear.HearFamily(Sound.SoundTypeEnum.B));
        }        
        
        [TestMethod]
        public void DistanceTest()
        {
            var environment = DawnOnline.Simulation.Environment.GetWorld();

            var creature = new CreatureOnClient(1);
            environment.AddCreature(creature, Vector2.Zero, 0);
            var ear = new Ear(creature, Vector2.Zero);

            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));

            // I hear you
            var farSound = new Sound(new Vector2(10, 0), Sound.SoundTypeEnum.A, creature.FamilyId, 10, 1);
            environment.AddSound(farSound);

            // I hear you better
            var nearSound = new Sound(new Vector2(5, 0), Sound.SoundTypeEnum.B, creature.FamilyId, 10, 1);
            environment.AddSound(nearSound);

            Assert.AreNotEqual(0, ear.HearFamily(Sound.SoundTypeEnum.A));
            Assert.AreNotEqual(0, ear.HearFamily(Sound.SoundTypeEnum.B));
            Assert.IsTrue(ear.HearFamily(Sound.SoundTypeEnum.A) < ear.HearFamily(Sound.SoundTypeEnum.B));
        }
    }
}
