using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    class SpawnPointBrain : AbstractBrain
    {
        private readonly double _maxInterval;
        private readonly EntityType _spawnType;
        private double _interval;
        private DateTime _lastSpawn;

        internal SpawnPointBrain(EntityType spawnType, double interval)
        {
            _spawnType = spawnType;
            _maxInterval = interval;
        }

        internal override void DoSomething()
        {
            if ((DateTime.Now - _lastSpawn).TotalSeconds < _interval)
                return;

            var randomizer = new Random((int) DateTime.Now.Ticks);
            var choice = randomizer.Next(2);
            if (choice == 0)
                SpawnHunter();
            if (choice == 1)
                SpawnProtector();

            _lastSpawn = DateTime.Now;
            _interval = Globals.Radomizer.NextDouble()*_maxInterval;
        }

        private void SpawnHunter()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature);
            Environment.GetWorld().AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        private void SpawnForager()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            creature.Brain = new PredatorBrain_Forager();
            creature.Brain.InitializeSenses();
            Environment.GetWorld().AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        private void SpawnProtector()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            creature.Brain = new PredatorBrain_Protector();
            creature.Brain.InitializeSenses();
            Environment.GetWorld().AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        internal override AbstractBrain Replicate()
        {
            return new SpawnPointBrain(_spawnType, _maxInterval);
        }
    }
}
