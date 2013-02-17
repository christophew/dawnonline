using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains.Neural;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.Simulation.Brains
{
    class SpawnPointBrain : AbstractBrain
    {
        private readonly EntityType _spawnType;

        private readonly double _maxSpawnCooldown;
        private double _currentSpawnCooldown;
        private DateTime _lastSpawn;

        internal ICreature PrototypeNeuralForager { get; set; }

        internal SpawnPointBrain(EntityType spawnType, double interval)
        {
            _spawnType = spawnType;
            _maxSpawnCooldown = interval;

            var prototype = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
            var prototypeBrain = new NeuralBrain();
            prototypeBrain.PredefineBehaviour();
            //prototypeBrain.PredefineRandomBehaviour();
            //var prototypeBrain = new PredatorBrain();
            prototype.Brain = prototypeBrain;
            PrototypeNeuralForager = prototype;
        }

        internal override void DoSomething(TimeSpan timeDelta)
        {
            // Need new energy
            if (MyCreature.IsTired)
                return;

            if ((DateTime.Now - _lastSpawn).TotalSeconds < _currentSpawnCooldown)
                return;

            // Score increase when we spawn
            //MyCreature.CharacterSheet.Score += 5;

            SpawnNeuralForager();

            //var choice = Globals.Radomizer.Next(3);
            //if (choice == 0 || choice == 1)
            //    SpawnForager();
            //if (choice == 2)
            //    SpawnProtector();

            // TODO: Should not be necessary, but can be useful to have faster update of monitor (instead of waiting for server update)
            MyCreature.CharacterSheet.Fatigue.Increase(20);

            _lastSpawn = DateTime.Now;
            _currentSpawnCooldown = Globals.Radomizer.NextDouble()*_maxSpawnCooldown;

            // RegisterSpawn, so Fatigue & Score can be updated on the server
            MyCreature.RegisterSpawn();
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
            var replicatedCreature = PrototypeNeuralForager.Replicate(PrototypeNeuralForager) as Creature;
            replicatedCreature.SpawnPoint = MyCreature;

            AddToWorld(replicatedCreature);
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

        internal override AbstractBrain Replicate(AbstractBrain mate)
        {
            Console.WriteLine("Generation: " + MyCreature.CharacterSheet.Generation);

            var newBrain = new SpawnPointBrain(_spawnType, _maxSpawnCooldown);

             // crossover
            var spawnPointMate = mate as SpawnPointBrain;
            Debug.Assert(spawnPointMate != null, "sodomy!");

           newBrain.PrototypeNeuralForager = PrototypeNeuralForager.Replicate(spawnPointMate.PrototypeNeuralForager);

            // MUTATE 
            newBrain.PrototypeNeuralForager.Mutate();

            return newBrain;
        }
    }
}
