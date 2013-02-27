using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public static bool IsObstacle(EntityType entityType)
        {
            return entityType == EntityType.Wall ||
                   entityType == EntityType.Box ||
                   entityType == EntityType.Treasure;
        }

        public static bool IsCreature(EntityType entityType)
        {
            return entityType == EntityType.Predator ||
                   entityType == EntityType.SpawnPoint ||
                   entityType == EntityType.Plant ||
                   entityType == EntityType.Rabbit ||
                   entityType == EntityType.Turret;
        }

        public static IEntity CreateObstacle(int id, EntityType entityType, double height, double wide)
        {
            switch (entityType)
            {
                case EntityType.Box:
                    {
                        var obstacle = ObstacleBuilder.CreateObstacleBox(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }
                case EntityType.Wall:
                    {
                        var obstacle = ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }
                case EntityType.Treasure:
                    {
                        var obstacle = ObstacleBuilder.CreateTreasure(false) as Obstacle;
                        Debug.Assert(obstacle != null);
                        obstacle.Id = id;
                        return obstacle;
                    }

                default: throw new NotImplementedException("TODO");
            }
        }

        public static ICreature CreateCreature(EntityType entityType, IEntity spawnPoint, int id)
        {
            var creature = CreatureBuilder.CreateCreature(entityType) as Creature;
            Debug.Assert(creature != null);
            creature.Id = id;

            Debug.Assert(spawnPoint == null || spawnPoint.Specy == EntityType.SpawnPoint);
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

        public static void UpdateStatus(IEntity entity, int damagePercent, int fatiguePercent, int score)
        {
            var creature = entity as Creature;
            if (creature == null)
                return;

            creature.CharacterSheet.Damage.PercentFilled = damagePercent;
            creature.CharacterSheet.Fatigue.PercentFilled = fatiguePercent;
            creature.CharacterSheet.Score = score;
        }
    }
}
