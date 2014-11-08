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
        private readonly double _maxSpawnCooldown = 30; // TODO: move to CharacterSheet?

        private double _currentSpawnCooldown;
        private DateTime _lastSpawn;

        // Some spawns to get the family going, before we have any resources gathered
        private int _freeSpawns = 2;


        internal ICreature PrototypeCreature { get; set; }

        internal SpawnPointBrain(ICreature prototype)
        {
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

        private void SpawnNeuralCreature()
        {
            var replicatedCreature = PrototypeCreature.Replicate(PrototypeCreature);

            // TODO: should work like this
            //Debug.Assert(replicatedCreature.SpawnPoint == MyCreature);
            // TODO => this is the shortcut
            replicatedCreature.SetSpawnPoint(MyCreature);

            AddToWorld(replicatedCreature);
        }

        private void AddToWorld(ICreature creature)
        {
            MyCreature.MyEnvironment.AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble() * Math.PI * 2.0, false);
        }

        public override IBrain Replicate(IBrain mate)
        {
            Console.WriteLine("Generation: " + MyCreature.CharacterSheet.Generation);

             // crossover
            var spawnPointMate = mate as SpawnPointBrain;
            Debug.Assert(spawnPointMate != null, "sodomy!");

            var newPrototype = PrototypeCreature.Replicate(spawnPointMate.PrototypeCreature);
            var newBrain = new SpawnPointBrain(newPrototype);


            // TODO: Mutate!

            return newBrain;
        }

        public override void Mutate()
        {
            PrototypeCreature.Mutate();
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)PrototypeCreature.EntityType);
            writer.Write(_maxSpawnCooldown);

            var brain = PrototypeCreature.Brain as AbstractBrain;
            Debug.Assert(brain != null, "TODO");

            brain.Serialize(writer);
        }

        public override void Deserialize(BinaryReader reader)
        {
            var spawnType = (EntityTypeEnum) reader.ReadInt32();
            Debug.Assert(spawnType == PrototypeCreature.EntityType, "Validate");
            var maxSpawnCooldown = reader.ReadDouble();
            Debug.Assert(maxSpawnCooldown == _maxSpawnCooldown, "Validate");

            var brain = PrototypeCreature.Brain as AbstractBrain;
            Debug.Assert(brain != null, "TODO");

            brain.Deserialize(reader);
        }
    }
}
