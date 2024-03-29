﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.Simulation.Senses
{
    internal class Eye : IEye
    {
        private readonly Creature _creature;

        internal Eye(Creature creature)
        {
            _creature = creature;
        }

        internal double Angle { get; set; }

        internal double VisionDistance { get; set; }
        internal double VisionAngleDelta { get; set; }

        private Environment CreatureEnvironment { get { return _creature.MyEnvironment; } }
        private Placement CreaturePlace { get { return _creature.Place; } }

        public double DistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true)
        {
            foreach (var entity in sortedEntities)
            {
                // sortedEntitities should be ascended sorted on distance
                if (_OutOfRange(entity))
                    break;

                if (_HasLineOfSight(entity, useLineOfSight))
                    return MathTools.GetDistance(CreaturePlace.Position, entity.Place.Position);
            }
            return -1;
        }

        public double WeightedDistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true)
        {
            var value = DistanceToFirstVisible(sortedEntities, useLineOfSight);
            return value < 0 ? 0 : 100.0 * (VisionDistance - value) / VisionDistance;
        }

        public bool SeesCreature(ICreature creature)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            return HasLineOfSight(myCreature, null);
        }

        public bool SeesACreature(List<CreatureTypeEnum> species, IEntity spawnPointToExclude)
        {
            return species.Any(specy => HasLineOfSight(specy, spawnPointToExclude));
        }

        public bool SeesACreature(List<CreatureTypeEnum> species)
        {
            return species.Any(specy => HasLineOfSight(specy, null));
        }

        public bool SeesACreature(CreatureTypeEnum specy)
        {
            return HasLineOfSight(specy, null);
        }

        public bool SeesAnObstacle(EntityTypeEnum entityType)
        {
            var obstacles = CreatureEnvironment.GetObstacles().Where(o => o.EntityType == entityType);

            foreach (var obstacle in obstacles)
            {
                if (obstacle.EntityType != entityType)
                    continue;
                if (_OutOfRange(obstacle))
                    continue;

                if (_HasLineOfSight(obstacle, true))
                    return true;
            }

            return false;
        }

        private bool HasLineOfSight(CreatureTypeEnum specy, IEntity spawnPointToExclude)
        {
            if (specy == CreatureTypeEnum.Unknown)
                return false;

            var creatures = CreatureEnvironment.GetCreatures(specy);
            foreach (Creature current in creatures)
            {
                var lineOfSight = HasLineOfSight(current, spawnPointToExclude);
                if (lineOfSight)
                    return true;
            }

            return false;
        }

        private bool HasLineOfSight(Creature current, IEntity spawnPointToExclude)
        {
            // It's me
            if (current.Equals(_creature))
                return false;

            // It's my family
            if (spawnPointToExclude != null &&
                spawnPointToExclude == current.SpawnPoint)
                return false;

            if (_OutOfRange(current))
                return false;

            return _HasLineOfSight(current, true);
        }

        private  bool _OutOfRange(IEntity current)
        {
            var visionDistance2 = VisionDistance*VisionDistance;
            double distance2 = MathTools.GetDistance2(CreaturePlace.Position, current.Place.Position);
            return (distance2 > visionDistance2);
        }

        private bool _HasLineOfSight(IEntity current, bool useLineOfSight)
        {
            // same position
            if (_creature.Place.Position == current.Place.Position)
            {
                return true;
            }

            // Check angle
            {

                double angle = MathTools.GetAngle(CreaturePlace.Position.X, CreaturePlace.Position.Y,
                                                  current.Place.Position.X, current.Place.Position.Y);

                float deltaAngle = MathHelper.WrapAngle((float) (angle - (_creature.Place.Angle + Angle)));

                if (Math.Abs(deltaAngle) > VisionAngleDelta)
                    return false;
            }

            if (!useLineOfSight)
                return true;

            // Check obstacles
            {
                _entityToFind = current;
                _entityBlocked = false;
                Environment.GetWorld().FarSeerWorld.RayCast(this.RayCastCallback2, _creature.Place.Position, current.Place.Position);
                if (!_entityBlocked)
                    return true;
            }


            return false;
        }

        private IEntity _entityToFind;
        private bool _entityBlocked;

        internal float RayCastCallback2(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            //return -1: ignore this fixture and continue
            //return 0: terminate the ray cast
            //return fraction: clip the ray to this point
            //return 1: don't clip the ray and continue

            var entity = fixture.UserData as IEntity;
            //if ((creature != _creatureToFind) && (creature != _creature))
            if (entity != _entityToFind)
            {
                _entityBlocked = true;
                return 0;
            }
            return -1;
        }
    }
}
