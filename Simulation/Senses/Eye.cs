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

namespace DawnOnline.Simulation.Senses
{
    internal class Eye
    {
        private readonly Creature _creature;

        internal Eye(Creature creature)
        {
            _creature = creature;
        }

        internal double Angle { get; set; }
        internal double VisionDistance { get; set; }
        internal double VisionAngle { get; set; }

        private Environment CreatureEnvironment { get { return _creature.MyEnvironment; } }
        private Placement CreaturePlace { get { return _creature.Place; } }

        internal bool SeesCreature(Creature creature)
        {
            return HasLineOfSight(creature, null);
        }

        internal bool SeesACreature(List<EntityType> species, IEntity spawnPointToExclude)
        {
            return species.Any(specy => HasLineOfSight(specy, spawnPointToExclude));
        }

        internal bool SeesACreature(List<EntityType> species)
        {
            return species.Any(specy => HasLineOfSight(specy, null));
        }

        internal bool SeesACreature(EntityType specy)
        {
            return HasLineOfSight(specy, null);
        }

        private bool HasLineOfSight(EntityType specy, IEntity spawnPointToExclude)
        {
            if (specy == EntityType.Unknown)
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

            // Check distance
            var visionDistance2 = VisionDistance * VisionDistance;
            {
                double distance2 = MathTools.GetDistance2(CreaturePlace.Position, current.Place.Position);
                if (distance2 == 0)
                    return true;
                if (distance2 > visionDistance2)
                    return false;
            }

            // Check angle
            {
                double angle = MathTools.GetAngle(CreaturePlace.Position.X, CreaturePlace.Position.Y,
                                                  current.Place.Position.X, current.Place.Position.Y);

                if (MathTools.NormalizeAngle(Math.Abs(angle - (_creature.Place.Angle + Angle))) > VisionAngle)
                    return false;
            }

            // Check obstacles
            {
                _creatureToFind = current;
                _creatureBlocked = false;
                Environment.GetWorld().FarSeerWorld.RayCast(this.RayCastCallback2, _creature.Place.Position, current.Place.Position);
                if (!_creatureBlocked)
                    return true;
            }


            return false;
        }

        private Creature _creatureToFind;
        private bool _creatureBlocked;

        internal float RayCastCallback2(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            //return -1: ignore this fixture and continue
            //return 0: terminate the ray cast
            //return fraction: clip the ray to this point
            //return 1: don't clip the ray and continue

            var creature = fixture.UserData as Creature;
            //if ((creature != _creatureToFind) && (creature != _creature))
            if (creature != _creatureToFind)
            {
                _creatureBlocked = true;
                return 0;
            }
            return -1;
        }
    }
}