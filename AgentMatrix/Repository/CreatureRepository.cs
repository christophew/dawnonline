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

            InitDir(CreatureTypeEnum.Predator);
            InitDir(CreatureTypeEnum.Predator2);
            InitDir(CreatureTypeEnum.Rabbit);

            _singleton = new CreatureRepository();
            _singleton.Load();
            return _singleton;
        }

        public static CreatureRepository GetRepository()
        {
            return CreateOrGetSingleton();
        }

        private static void InitDir(CreatureTypeEnum creatureType)
        {
            if (!Directory.Exists(GetPath(creatureType)))
            {
                Directory.CreateDirectory(GetPath(creatureType));
            }
        }

        private readonly List<CreatureRepositoryEntry> _repository = new List<CreatureRepositoryEntry>();
        private readonly List<CreatureRepositoryEntry> _repository2 = new List<CreatureRepositoryEntry>();
        private readonly List<CreatureRepositoryEntry> _repositoryRabbit = new List<CreatureRepositoryEntry>();

        public void Add(ICreature creature)
        {
            Debug.Assert(!(creature.Brain is DummyBrain), "We are only interested in real creatures, created in this AgentMatrix");

            var repository = GetRepository(creature.CreatureType);

            repository.Add(new CreatureRepositoryEntry(creature));
        }

        public List<ICreature> GetSortedRelevantSpawnpoints(CreatureTypeEnum spawnPointType)
        {
            Debug.Assert(spawnPointType == CreatureTypeEnum.Predator || spawnPointType == CreatureTypeEnum.Predator2 || spawnPointType == CreatureTypeEnum.Rabbit);

            var repository = GetRepository(spawnPointType);

            //var list = _repository
            //    .Where(entry => entry.Alive && (entry.Creature.Specy == spawnPointType));
            var list = repository
                .Where(entry => entry.Creature.CreatureType == spawnPointType);

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

        private static string GetPath(CreatureTypeEnum creatureType)
        {
            if (creatureType == CreatureTypeEnum.Predator)
                return _path + @"\PredatorSpawnPoint\";
            if (creatureType == CreatureTypeEnum.Predator2)
                return _path + @"\PredatorSpawnPoint2\";
            if (creatureType == CreatureTypeEnum.Rabbit)
                return _path + @"\RabbitSpawnPoint\";

            throw new NotSupportedException();
        }

        private List<CreatureRepositoryEntry> GetRepository(CreatureTypeEnum creatureType)
        {
            if (creatureType == CreatureTypeEnum.Predator)
                return _repository;
            if (creatureType == CreatureTypeEnum.Predator2)
                return _repository2;
            if (creatureType == CreatureTypeEnum.Rabbit)
                return _repositoryRabbit;

            throw new NotSupportedException();
        }

        public void Save()
        {
            Save(CreatureTypeEnum.Predator);
            Save(CreatureTypeEnum.Predator2);
            Save(CreatureTypeEnum.Rabbit);
        }

        private void Save(CreatureTypeEnum creatureType)
        {
            var path = GetPath(creatureType);
            var repository = GetRepository(creatureType);

            foreach (var entry in repository)
            {
                entry.Save(path);
            }
        }

        public void Load()
        {
            Load(CreatureTypeEnum.Predator);
            Load(CreatureTypeEnum.Predator2);
            Load(CreatureTypeEnum.Rabbit);
        }

        private void Load(CreatureTypeEnum creatureType)
        {
            var path = GetPath(creatureType);
            var repository = GetRepository(creatureType);

            var fileNames = Directory.EnumerateFiles(path);

            foreach (var fileName in fileNames)
            {
                var loadedEntry = new CreatureRepositoryEntry(creatureType, fileName);

                // TODO: currently this is handled in c'tor CreatureRepositoryEntry
                // => needs to change
                //repository.Add(loadedEntry);
            }
        }
    }
}
