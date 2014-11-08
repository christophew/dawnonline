using System;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.AgentMatrix.Brains.Neural;
using DawnOnline.AgentMatrix.Repository;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Factories
{
    static class AgentCreatureBuilder
    {
        private static AbstractBrainFactory _brainFactory;

        public static void SetBrainFactory(AbstractBrainFactory brainFactory)
        {
            _brainFactory = brainFactory;
        }

        public static ICreature CreateSpawnPoint(CreatureTypeEnum specy)
        {
            if (_brainFactory == null)
                throw new InvalidOperationException("BrainFactory not set");

            var prototypeBrain = _brainFactory.CreateBrainFor(specy);
            var prototype = CreatureBuilder.CreateCreature(EntityTypeEnum.Creature, specy, prototypeBrain);
            var spawnPointBrain = _brainFactory.CreateSpawnPointBrain(prototype);
            var spawnPoint = CreatureBuilder.CreateCreature(EntityTypeEnum.SpawnPoint, specy, spawnPointBrain);

            return spawnPoint;
        }

        private static ICreature CreateSpawnPoint()
        {
            var prototypeBrain = _brainFactory.CreateBrainFor(CreatureTypeEnum.Predator);
            var prototype = CreatureBuilder.CreatePredator(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateSpawnPoint(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }

        private static ICreature CreateSpawnPoint2()
        {
            var prototypeBrain = _brainFactory.CreateBrainFor(CreatureTypeEnum.Predator2);
            var prototype = CreatureBuilder.CreatePredator2(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateSpawnPoint2(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }

        private static ICreature CreateRabbitSpawnPoint()
        {
            var prototypeBrain = _brainFactory.CreateBrainFor(CreatureTypeEnum.Rabbit);
            var prototype = CreatureBuilder.CreateRabbit(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateRabbitSpawnPoint(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }
    }
}
