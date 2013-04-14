using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains.Neural;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Brains
{
    class SpawnPointBrain : AbstractBrain
    {
        private readonly EntityType _spawnType;
        private readonly double _maxSpawnCooldown;

        private double _currentSpawnCooldown;
        private DateTime _lastSpawn;

        // Some spawns to get the family going, before we have any resources gathered
        private int _freeSpawns = 2;


        internal ICreature PrototypeCreature { get; set; }

        internal SpawnPointBrain(EntityType spawnType, double interval)
        {
            _spawnType = spawnType;
            _maxSpawnCooldown = interval;

            var prototypeBrain = new NeuralBrain();
            prototypeBrain.PredefineBehaviour();
            var prototype = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature, prototypeBrain);
            //prototypeBrain.PredefineRandomBehaviour();
            //var prototypeBrain = new PredatorBrain();

            PrototypeCreature = prototype;
        }

        public override void DoSomething(TimeSpan timeDelta)
        {
            if ((DateTime.Now - _lastSpawn).TotalSeconds < _currentSpawnCooldown)
                return;

            // Need new energy
            if (MyCreature.IsTired)
                return;

            // Enough resources?
            if ((_freeSpawns-- <= 0) && (MyCreature.CharacterSheet.Resource.PercentFilled < 10))
                return;

            SpawnNeuralCreature();

            //var choice = Globals.Radomizer.Next(3);
            //if (choice == 0 || choice == 1)
            //    SpawnForager();
            //if (choice == 2)
            //    SpawnProtector();

            // TODO: Should not be necessary, but can be useful to have faster update of monitor (instead of waiting for server update)
            //MyCreature.CharacterSheet.Fatigue.Increase(20);

            _lastSpawn = DateTime.Now;
            _currentSpawnCooldown = Globals.Radomizer.NextDouble()*_maxSpawnCooldown;

            // RegisterSpawn, so Status can be updated on the server
            MyCreature.RegisterSpawn();
        }

        //private void SpawnHunter()
        //{
        //    var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature);
        //    AddToWorld(creature);
        //}

        //private void SpawnForager()
        //{
        //    var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
        //    creature.Brain = new ForagerBrain();
        //    AddToWorld(creature);
        //}

        private void SpawnNeuralCreature()
        {
            var replicatedCreature = PrototypeCreature.Replicate(PrototypeCreature);

            // TODO: should work like this
            //Debug.Assert(replicatedCreature.SpawnPoint == MyCreature);
            // TODO => this is the shortcut
            replicatedCreature.SetSpawnPoint(MyCreature);

            AddToWorld(replicatedCreature);
        }

        //private void SpawnProtector()
        //{
        //    var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature) as Creature;
        //    creature.Brain = new ProtectorBrain();
        //    AddToWorld(creature);
        //}

        private void AddToWorld(ICreature creature)
        {
            MyCreature.MyEnvironment.AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        public override IBrain Replicate(IBrain mate)
        {
            Console.WriteLine("Generation: " + MyCreature.CharacterSheet.Generation);

            var newBrain = new SpawnPointBrain(_spawnType, _maxSpawnCooldown);

             // crossover
            var spawnPointMate = mate as SpawnPointBrain;
            Debug.Assert(spawnPointMate != null, "sodomy!");

           newBrain.PrototypeCreature = PrototypeCreature.Replicate(spawnPointMate.PrototypeCreature);

            // MUTATE 
            //newBrain.PrototypeNeuralForager.Mutate();

            return newBrain;
        }

        public override void Mutate()
        {
            PrototypeCreature.Mutate();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((int)_spawnType);
            writer.Write(_maxSpawnCooldown);

            var neuralBrain = PrototypeCreature.Brain as NeuralBrain;
            Debug.Assert(neuralBrain != null, "TODO");

            neuralBrain.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            var spawnType = (EntityType) reader.ReadInt32();
            Debug.Assert(spawnType == _spawnType, "Validate");
            var maxSpawnCooldown = reader.ReadDouble();
            Debug.Assert(maxSpawnCooldown == _maxSpawnCooldown, "Validate");

            var neuralBrain = PrototypeCreature.Brain as NeuralBrain;
            Debug.Assert(neuralBrain != null, "TODO");

            neuralBrain.Deserialize(reader);
        }
    }
}
