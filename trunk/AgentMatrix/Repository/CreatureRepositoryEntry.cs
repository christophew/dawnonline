﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.AgentMatrix.Brains.Neural;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Repository
{
    class CreatureRepositoryEntry
    {
        public ICreature Creature { get; private set; }
        public bool IsSaved { get; private set; }

        public bool Alive
        {
            get
            {
                // Creatures are only saved on death
                if (IsSaved)
                    return false;
                return Creature.Alive;
            }
        }

        public CreatureRepositoryEntry(ICreature creature)
        {
            Creature = creature;
        }

        public CreatureRepositoryEntry(EntityType entityType, string fileName)
        {
            Load(entityType, fileName);
        }

        public void Save(string path)
        {
            if (Creature.IsSpawnPoint)
                SaveSpawnPoint(path);
            else
            {
                throw new NotSupportedException();
            }
        }

        private void SaveSpawnPoint(string path)
        {
            // Only save once
            if (IsSaved)
                return;
            // Only save after death = score will no longer change
            if (Alive)
                return;


            var fileName = Creature.CharacterSheet.Generation + "-" + Creature.CharacterSheet.Score + "-" + Guid.NewGuid() + ".dwn";

            using (var stream = new FileStream(path + fileName, FileMode.CreateNew))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((int)Creature.Specy);
                    writer.Write(Creature.CharacterSheet.Score);
                    writer.Write(Creature.CharacterSheet.Generation);

                    // Brain
                    var brain = Creature.Brain as SpawnPointBrain;
                    Debug.Assert(brain != null, "TODO");
                    brain.Serialize(writer);
                }
            }

            IsSaved = true;
        }

        private void Load(EntityType entityType, string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open);

            using (var reader = new BinaryReader(stream))
            {
                // Initialize SpawnPoint
                var newSpawnPoint = AgentCreatureBuilder.CreateCreature(entityType);

                // Restore SpawnPoint
                var specy = (EntityType)reader.ReadInt32();
                Debug.Assert(specy == entityType, "Validation");
                var savedScore = reader.ReadDouble();
                var savedGeneration = reader.ReadInt32();
                newSpawnPoint.CharacterSheet.Restore(savedScore, savedGeneration);

                var brain = newSpawnPoint.Brain as SpawnPointBrain;
                Debug.Assert(brain != null, "TODO");
                brain.Deserialize(reader);

                // Entry properties
                Creature = newSpawnPoint;
                IsSaved = true;
            }
        }
    }
}