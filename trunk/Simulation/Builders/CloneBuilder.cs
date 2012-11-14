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
                   entityType == EntityType.Box;
        }

        public static bool IsCreature(EntityType entityType)
        {
            return entityType == EntityType.Predator;
        }

        public static IEntity CreateObstacle(int id, EntityType entityType, double height, double wide)
        {
            switch (entityType)
            {
                case EntityType.Box:
                    {
                        var obstacle = ObstacleBuilder.CreateObstacleBox(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        return obstacle;
                    }
                case EntityType.Wall:
                    {
                        var obstacle = ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide) as Obstacle;
                        Debug.Assert(obstacle != null);
                        return obstacle;
                    }

                default: throw new NotImplementedException("TODO");
            }
        }

        public static IEntity CreateCreature(int id, EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Predator:
                    {
                        var creature = CreatureBuilder.CreatePredator() as Creature;
                        Debug.Assert(creature != null);
                        return creature;
                    }

                default: throw new NotImplementedException("TODO");
            }
        }

        public static void UpdatePosition(IEntity entity, Vector2 position, double angle)
        {
            entity.Place.OffsetPosition(position, angle);
        }
    }
}
