using System;
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

        internal bool SeesACreature(List<EntityType> species)
        {
            return species.Any(HasLineOfSight);
        }

        internal bool SeesACreature(EntityType specy)
        {
            return HasLineOfSight(specy);
        }

        private bool HasLineOfSight(EntityType specy)
        {
            if (specy == EntityType.Unknown)
                return false;

            var creatures = CreatureEnvironment.GetCreatures(specy);
            foreach (Creature current in creatures)
            {
                var lineOfSight = HasLineOfSight(current);
                if (lineOfSight)
                    return true;
            }

            return false;
        }

        private bool HasLineOfSight(Creature current)
        {
            // It's me
            if (current.Equals(_creature))
                return false;

            // Check distance
            var visionDistance2 = VisionDistance * VisionDistance;
            {
                double distance2 = MathTools.GetDistance2(CreaturePlace.Position, current.Place.Position);
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
            _fixturesInRay = new List<Fixture>();
            var me = this;
            Environment.GetWorld().FarSeerWorld.RayCast(me.RayCastCallback, _creature.Place.Position, current.Place.Position);

            // Fixtures are NOT hit by the ray in sequence
            // = we'll have to find the closest entity that was hit ourselves
            Fixture closestFixture = FindClosestFixture(_creature.Place.Position, _fixturesInRay);

            var closestEntity = closestFixture != null ? closestFixture.UserData as IEntity : null;
            if (closestEntity == current)
                return true;


            return false;
        }

        private List<Fixture> _fixturesInRay;

        internal float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            //return -1: ignore this fixture and continue
            //return 0: terminate the ray cast
            //return fraction: clip the ray to this point
            //return 1: don't clip the ray and continue

            _fixturesInRay.Add(fixture);
            return -1;
        }

        private static Fixture FindClosestFixture(Vector2 origin, IEnumerable<Fixture> fixtures)
        {
            Fixture closestFixture = null;
            double closestDistance = float.MaxValue;
            foreach (var fixture in fixtures)
            {
                var currentDistance = MathTools.GetDistance(origin, fixture.Body.Position);
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestFixture = fixture;
                }
            }

            return closestFixture;
        }
    }
}
