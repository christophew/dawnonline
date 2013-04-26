using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.Simulation.Builders
{
    /// <summary>
    /// Used to recreate a Simution world starting from another simution world
    /// </summary>
    public static class CloneBuilder
    {
        public static bool IsObstacle(EntityTypeEnum entityType)
        {
            return entityType == EntityTypeEnum.Wall ||
                   entityType == EntityTypeEnum.Box ||
                   entityType == EntityTypeEnum.Treasure; // TODO: treasure = Creature->Food?
        }

        public static bool IsCreature(EntityTypeEnum entityType)
        {
            return entityType == EntityTypeEnum.Creature ||
                   entityType == EntityTypeEnum.SpawnPoint;
        }

        public static IEntity CreateObstacle(int id, EntityTypeEnum entityType, CreatureTypeEnum creatureType, double height, double wide)
        {
            switch (entityType)
            {
                case EntityTypeEnum.Box:
                    {
                        var obstacle = ObstacleBuilder.CreateObstacleBox(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }
                case EntityTypeEnum.Wall:
                    {
                        var obstacle = ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }
                case EntityTypeEnum.Treasure:
                    {
                        var obstacle = ObstacleBuilder.CreateTreasure(creatureType, false) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }

                default: throw new NotImplementedException("TODO");
            }
        }

        public static ICreature CreateCreature(EntityTypeEnum entityType, CreatureTypeEnum creatureType, ICreature spawnPoint, int id)
        {
            var creature = CreatureBuilder.CreateCreature(entityType, creatureType, new DummyBrain()) as Creature;
            Debug.Assert(creature != null);
            creature.Id = id;

            Debug.Assert(spawnPoint == null || spawnPoint.IsSpawnPoint);
            if (spawnPoint != null)
            {
                creature.SpawnPoint = spawnPoint;
            }

            return creature;
        }

        public static void UpdatePosition(IEntity entity, Vector2 position, double angle)
        {
            entity.Place.OffsetPosition(position, angle);
        }

        public static void UpdateStatus(IEntity entity, int damagePercent, int fatiguePercent, int resourcePercent, int score)
        {
            var creature = entity as Creature;
            if (creature == null)
                return;

            creature.CharacterSheet.Damage.PercentFilled = damagePercent;
            creature.CharacterSheet.Fatigue.PercentFilled = fatiguePercent;
            creature.CharacterSheet.Resource.PercentFilled = resourcePercent;
            creature.CharacterSheet.Score = score;
        }
    }
}
