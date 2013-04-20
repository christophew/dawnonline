using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Repository
{
    class CreatureRepository
    {
        private const string _path = @".\CreatureRepository\";

        private static CreatureRepository _singleton;
        private static CreatureRepository CreateOrGetSingleton()
        {
            if (_singleton != null)
                return _singleton;

            InitDir(EntityType.SpawnPoint1);
            InitDir(EntityType.SpawnPoint2);

            _singleton = new CreatureRepository();
            _singleton.Load();
            return _singleton;
        }

        public static CreatureRepository GetRepository()
        {
            return CreateOrGetSingleton();
        }

        private static void InitDir(EntityType entityType)
        {
            if (!Directory.Exists(GetPath(entityType)))
            {
                Directory.CreateDirectory(GetPath(entityType));
            }
        }

        private readonly List<CreatureRepositoryEntry> _repository = new List<CreatureRepositoryEntry>();
        private readonly List<CreatureRepositoryEntry> _repository2 = new List<CreatureRepositoryEntry>();

        public void Add(ICreature creature)
        {
            Debug.Assert(!(creature.Brain is DummyBrain), "We are only interested in real creatures, created in this AgentMatrix");

            var repository = GetRepository(creature.Specy);

            repository.Add(new CreatureRepositoryEntry(creature));
        }

        public List<ICreature> GetSortedRelevantSpawnpoints(EntityType spawnPointType)
        {
            Debug.Assert(spawnPointType == EntityType.SpawnPoint1 || spawnPointType == EntityType.SpawnPoint2);

            var repository = GetRepository(spawnPointType);

            //var list = _repository
            //    .Where(entry => entry.Alive && (entry.Creature.Specy == spawnPointType));
            var list = repository
                .Where(entry => entry.Creature.Specy == spawnPointType);

            // Take youngest
            var sortedOnGeneration = list
                .OrderByDescending(entry => entry.Creature.CharacterSheet.Generation)
                .Take(100);

            // Sort on best score
            var sorted = sortedOnGeneration
                .OrderByDescending(entry => entry.Creature.CharacterSheet.Score)
                .Select(entry => entry.Creature)
                .ToList();

            return sorted;
        }

        private static string GetPath(EntityType entityType)
        {
            if (entityType == EntityType.SpawnPoint1)
                return _path + @"\SpawnPoint1\";
            if (entityType == EntityType.SpawnPoint2)
                return _path + @"\SpawnPoint2\";

            throw new NotSupportedException();
        }

        private List<CreatureRepositoryEntry> GetRepository(EntityType entityType)
        {
            if (entityType == EntityType.SpawnPoint1)
                return _repository;
            if (entityType == EntityType.SpawnPoint2)
                return _repository2;

            throw new NotSupportedException();
        }

        public void Save()
        {
            Save(EntityType.SpawnPoint1);
            Save(EntityType.SpawnPoint2);
        }

        private void Save(EntityType entityType)
        {
            var path = GetPath(entityType);
            var repository = GetRepository(entityType);

            foreach (var entry in repository)
            {
                entry.Save(path);
            }
        }

        public void Load()
        {
            Load(EntityType.SpawnPoint1);
            Load(EntityType.SpawnPoint2);
        }

        private void Load(EntityType entityType)
        {
            var path = GetPath(entityType);
            var repository = GetRepository(entityType);

            var fileNames = Directory.EnumerateFiles(path);

            foreach (var fileName in fileNames)
            {
                var loadedEntry = new CreatureRepositoryEntry(entityType, fileName);

                // TODO: currently this is handled in c'tor CreatureRepositoryEntry
                // => needs to change
                //repository.Add(loadedEntry);
            }
        }
    }
}
