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

            InitDir(EntityType.PredatorSpawnPoint);
            InitDir(EntityType.PredatorSpawnPoint2);
            InitDir(EntityType.RabbitSpawnPoint);

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
        private readonly List<CreatureRepositoryEntry> _repositoryRabbit = new List<CreatureRepositoryEntry>();

        public void Add(ICreature creature)
        {
            Debug.Assert(!(creature.Brain is DummyBrain), "We are only interested in real creatures, created in this AgentMatrix");

            var repository = GetRepository(creature.Specy);

            repository.Add(new CreatureRepositoryEntry(creature));
        }

        public List<ICreature> GetSortedRelevantSpawnpoints(EntityType spawnPointType)
        {
            Debug.Assert(spawnPointType == EntityType.PredatorSpawnPoint || spawnPointType == EntityType.PredatorSpawnPoint2 || spawnPointType == EntityType.RabbitSpawnPoint);

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
            if (entityType == EntityType.PredatorSpawnPoint)
                return _path + @"\PredatorSpawnPoint\";
            if (entityType == EntityType.PredatorSpawnPoint2)
                return _path + @"\PredatorSpawnPoint2\";
            if (entityType == EntityType.RabbitSpawnPoint)
                return _path + @"\RabbitSpawnPoint\";

            throw new NotSupportedException();
        }

        private List<CreatureRepositoryEntry> GetRepository(EntityType entityType)
        {
            if (entityType == EntityType.PredatorSpawnPoint)
                return _repository;
            if (entityType == EntityType.PredatorSpawnPoint2)
                return _repository2;
            if (entityType == EntityType.RabbitSpawnPoint)
                return _repositoryRabbit;

            throw new NotSupportedException();
        }

        public void Save()
        {
            Save(EntityType.PredatorSpawnPoint);
            Save(EntityType.PredatorSpawnPoint2);
            Save(EntityType.RabbitSpawnPoint);
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
            Load(EntityType.PredatorSpawnPoint);
            Load(EntityType.PredatorSpawnPoint2);
            Load(EntityType.RabbitSpawnPoint);
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
