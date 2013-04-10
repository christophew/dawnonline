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

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            _singleton = new CreatureRepository();
            _singleton.Load();
            return _singleton;
        }

        public static CreatureRepository GetRepository()
        {
            return CreateOrGetSingleton();
        }

        private readonly List<CreatureRepositoryEntry> _repository = new List<CreatureRepositoryEntry>();

        public void Add(ICreature creature)
        {
            Debug.Assert(!(creature.Brain is DummyBrain), "We are only interested in real creatures, created in this AgentMatrix");

            _repository.Add(new CreatureRepositoryEntry(creature));
        }

        public List<ICreature> GetSortedRelevantSpawnpoints()
        {
            //var list = _repository
            //    .Where(entry => entry.Alive && (entry.Creature.Specy == EntityType.SpawnPoint));
            var list = _repository
                .Where(entry => entry.Creature.Specy == EntityType.SpawnPoint);

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

        public void Save()
        {
            foreach (var entry in _repository)
            {
                entry.Save(_path);
            }
        }

        public void Load()
        {
            var fileNames = Directory.EnumerateFiles(_path);

            foreach (var fileName in fileNames)
            {
                var loadedEntry = new CreatureRepositoryEntry(fileName);
                _repository.Add(loadedEntry);
            }
        }
    }
}
