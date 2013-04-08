using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix
{
    static class AgentCreatureBuilder
    {
        public static ICreature CreateSpawnPoint()
        {
            var spawnPointBrain = new SpawnPointBrain(EntityType.Predator, 30);
            return CreatureBuilder.CreateSpawnPoint(EntityType.Predator, spawnPointBrain);
        }
    }
}
