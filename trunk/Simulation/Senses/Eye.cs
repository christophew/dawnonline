using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.Senses
{
    internal class Eye : IEye
    {
        private Creature _creature;

        public Eye(Creature creature)
        {
            _creature = creature;
        }

        public double Angle { get; set; }
        public double VisionDistance { get; set; }
        public double VisionAngle { get; set; }

        private Environment CreatureEnvironment { get { return _creature.MyEnvironment; } }
        private IPlacement CreaturePlace { get { return _creature.Place; } }

        public bool SeesACreature()
        {
            return GetLineOfSight() != null;
        }

        public bool Sees(Creature targetCreature)
        {
            return GetLineOfSight(targetCreature) != null;
        }

        public IPolygon GetLineOfSight()
        {
            var creatures = CreatureEnvironment.GetCreatures();
            foreach (Creature current in creatures)
            {
                var lineOfSight = GetLineOfSight(current);
                if (lineOfSight != null)
                    return lineOfSight;
            }

            return null;
        }

        public IPolygon GetLineOfSight(Creature current)
        {
            // It's me
            if (current.Equals(_creature))
                return null;

            // Check distance
            {
                double distance2 = MathTools.GetDistance2(CreaturePlace.Position, current.Place.Position);
                if (distance2 > VisionDistance*VisionDistance)
                    return null;
            }

            // Check angle
            {
                double angle = MathTools.GetAngle(CreaturePlace.Position.X, CreaturePlace.Position.Y,
                                                  current.Place.Position.X, current.Place.Position.Y);

                if (MathTools.NormalizeAngle(Math.Abs(angle - (_creature.Place.Angle + Angle))) > VisionAngle)
                    return null;
            }

            // Check obstacles
            Polygon lineOfSight = new Polygon();
            {
                lineOfSight.Points.Add(new Vector(0, 0));
                lineOfSight.Points.Add(new Vector((float) (current.Place.Position.X - CreaturePlace.Position.X),
                                                  (float) (current.Place.Position.Y - CreaturePlace.Position.Y)));
                lineOfSight.BuildEdges();
                lineOfSight.Offset((float) CreaturePlace.Position.X, (float) CreaturePlace.Position.Y);

                bool visionBlocked = false;
                foreach (var obstacle in CreatureEnvironment.GetObstacles())
                {
                    Polygon obstaclePolygon = obstacle.Form.Shape as Polygon;
                    PolygonCollisionResult collitionResult = CollisionDetection.PolygonCollision(lineOfSight,
                                                                                                 obstaclePolygon,
                                                                                                 new Vector());

                    if (collitionResult.Intersect)
                    {
                        visionBlocked = true;
                        break;
                    }
                }
                if (visionBlocked)
                    return null;
            }

            return lineOfSight;
    }
    }
}
