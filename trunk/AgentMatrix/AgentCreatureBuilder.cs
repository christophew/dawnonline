﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.AgentMatrix.Brains.Neural;
using DawnOnline.AgentMatrix.Repository;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix
{
    static class AgentCreatureBuilder
    {
        public static ICreature CreateSpawnPoint(CreatureTypeEnum specy)
        {
            switch (specy)
            {
                case CreatureTypeEnum.Predator:
                    return CreateSpawnPoint();
                case CreatureTypeEnum.Predator2:
                    return CreateSpawnPoint2();
                case CreatureTypeEnum.Rabbit:
                    return CreateRabbitSpawnPoint();
            }

            throw new NotSupportedException();
        }

        public static ICreature CreateSpawnPoint()
        {
            var prototypeBrain = new NeuralBrain();
            prototypeBrain.PredefineBehaviour();
            var prototype = CreatureBuilder.CreatePredator(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateSpawnPoint(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }

        public static ICreature CreateSpawnPoint2()
        {
            var prototypeBrain = new NeuralBrain();
            prototypeBrain.PredefineBehaviour();
            var prototype = CreatureBuilder.CreatePredator2(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateSpawnPoint2(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }

        public static ICreature CreateRabbitSpawnPoint()
        {
            var prototypeBrain = new NeuralBrain();
            prototypeBrain.PredefineBehaviour();
            var prototype = CreatureBuilder.CreateRabbit(prototypeBrain);

            var spawnPointBrain = new SpawnPointBrain(prototype);
            var newSpawnPoint = CreatureBuilder.CreateRabbitSpawnPoint(spawnPointBrain);

            CreatureRepository.GetRepository().Add(newSpawnPoint);

            return newSpawnPoint;
        }
    }
}
