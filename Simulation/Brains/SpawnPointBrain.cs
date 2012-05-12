using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains.Neural;
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

        internal ICreature PrototypeNeuralForager { get; set; }

        internal SpawnPointBrain(EntityType spawnType, double interval)
        {
            _spawnType = spawnType;
            _maxInterval = interval;

            var _prototype = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            _prototype.Brain = new NeuralBrain();
            PrototypeNeuralForager = _prototype;
        }

        internal override void DoSomething(TimeSpan timeDelta)
        {
            // Need new energy
            if (MyCreature.IsTired)
                return;

            if ((DateTime.Now - _lastSpawn).TotalSeconds < _interval)
                return;

            // Score increase when we spawn
            //MyCreature.CharacterSheet.Score += 5;

            SpawnNeuralForager();

            //var choice = Globals.Radomizer.Next(3);
            //if (choice == 0 || choice == 1)
            //    SpawnForager();
            //if (choice == 2)
            //    SpawnProtector();

            MyCreature.CharacterSheet.Fatigue.Increase(20);

            _lastSpawn = DateTime.Now;
            _interval = Globals.Radomizer.NextDouble()*_maxInterval;
        }

        private void SpawnHunter()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature);
            AddToWorld(creature);
        }

        private void SpawnForager()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            creature.Brain = new ForagerBrain();
            AddToWorld(creature);
        }

        private void SpawnNeuralForager()
        {
            var replicatedCreature = PrototypeNeuralForager.Replicate() as Creature;
            replicatedCreature.SpawnPoint = MyCreature;

            AddToWorld(replicatedCreature);

            // Score
            this.MyCreature.CharacterSheet.Score += 10;
        }

        private void SpawnProtector()
        {
            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            creature.Brain = new ProtectorBrain();
            AddToWorld(creature);
        }

        private void AddToWorld(ICreature creature)
        {
            Environment.GetWorld().AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        internal override AbstractBrain Replicate()
        {
            Console.WriteLine("Generation: " + MyCreature.CharacterSheet.Generation);

            var newBrain = new SpawnPointBrain(_spawnType, _maxInterval);
            newBrain.PrototypeNeuralForager = PrototypeNeuralForager.Replicate();

            // MUTATE 
            newBrain.PrototypeNeuralForager.Mutate();

            return newBrain;
        }
    }
}
