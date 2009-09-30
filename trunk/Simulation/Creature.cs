using System;
using System.Diagnostics;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Statistics;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    class Creature : ICreature
    {
        private Placement _place = new Placement();
        private CharacterSheet _characterSheet = new CharacterSheet();
        private AbstractBrain _brain;

        public Environment MyEnvironment { get; set; }
        public IPlacement Place { get { return _place; } }

        public bool Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }

        public bool HasBrain { get { return Brain != null; } }


        internal AbstractBrain Brain 
        { 
            get { return _brain; }
            set
            {
                _brain = value;
                _brain.MyCreature = this;
            }
        }

        internal CharacterSheet Statistics { get { return _characterSheet; } }

        internal bool _alive = true;


        public Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
        }

        public bool IsTired
        {
            get
            {
                return !Statistics.Fatigue.CanIncrease((int) Statistics.FatigueCost);
            }
        }

        public void Rest()
        {
            Statistics.Fatigue.Decrease((int)Statistics.FatigueRecovery);
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            var originalTranslation = new Vector((float)(Math.Cos(_place.Angle) * Statistics.WalkingDistance),
                                                (float)(Math.Sin(_place.Angle) * Statistics.WalkingDistance));

            var realTranslation = TryMove(originalTranslation);

            _place.Position.X += realTranslation.X;
            _place.Position.Y += realTranslation.Y;
            (_place.Form.Shape as Polygon).Offset(realTranslation);
        }

        public void RunForward()
        {
            Debug.Assert(MyEnvironment != null);

            if (IsTired)
            {
                WalkForward();
                return;
            }

            var originalTranslation = new Vector((float)(Math.Cos(_place.Angle)*Statistics.RunningDistance),
                                                 (float)(Math.Sin(_place.Angle)*Statistics.RunningDistance));

            var realTranslation = TryMove(originalTranslation);

            _place.Position.X += realTranslation.X;
            _place.Position.Y += realTranslation.Y;
            (_place.Form.Shape as Polygon).Offset(realTranslation);

            Statistics.Fatigue.Increase((int)Statistics.FatigueCost);
        }

        private Vector TryMove(Vector velocity)
        {
            foreach (var obstacle in MyEnvironment.GetObstacles())
            {
                //if (polygon == player) continue;

                Polygon obstaclePolygon = obstacle.Form.Shape as Polygon;

                PolygonCollisionResult collitionResult = CollisionDetection.PolygonCollision(Place.Form.Shape as Polygon, obstaclePolygon, velocity);

                if (collitionResult.WillIntersect)
                {
                    return velocity + collitionResult.MinimumTranslationVector;
                }
            }

            return velocity;
        }

        public void TurnLeft()
        {
            _place.Angle -= Statistics.TurningAngle;
        }

        public void TurnRight()
        {
            _place.Angle += Statistics.TurningAngle;
        }

        public void Move()
        {
            Debug.Assert(Alive);

            if (!HasBrain)
                return;

            Brain.DoSomething();
        }

        public ICreature Attack()
        {
            Debug.Assert(Alive);

            return FindCreatureInCircle(_place.Position, _place.Form.Radius);
        }

        private static bool IsInCircle(Creature enemy, Coordinate position, double radius)
        {
            double deltaX = position.X - enemy.Place.Position.X;
            double deltaY = position.Y - enemy.Place.Position.Y;

            double distance2 = deltaX*deltaX + deltaY*deltaY;

            return distance2 < radius * radius;
        }

        private Creature FindCreatureInCircle(Coordinate center, double radius)
        {
            var creatures = MyEnvironment.GetCreatures();
            foreach (Creature current in creatures)
            {
                if (current.Equals(this))
                    continue;

                if (IsInCircle(current, center, radius))
                    return current;
            }

            return null;
        }

        public bool SeesACreatureForward()
        {
            double forwardRadius = Statistics.VisionRadius;

            var visionCenter = new Coordinate
            {
                X = _place.Position.X + Math.Cos(_place.Angle) * (forwardRadius + 10),
                Y = _place.Position.Y + Math.Sin(_place.Angle) * (forwardRadius + 10)
            };

            return FindCreatureInCircle(visionCenter, forwardRadius) != null;
        }

        public bool SeesACreatureLeft()
        {
            var visionCenter = new Coordinate
            {
                X = _place.Position.X + Math.Cos(_place.Angle - Math.PI / 3.0) * Statistics.VisionRadius,
                Y = _place.Position.Y + Math.Sin(_place.Angle - Math.PI / 3.0) * Statistics.VisionRadius
            };

            return FindCreatureInCircle(visionCenter, Statistics.VisionRadius) != null;
        }

        public bool SeesACreatureRight()
        {
            var visionCenter = new Coordinate
            {
                X = _place.Position.X + Math.Cos(_place.Angle + Math.PI / 3.0) * Statistics.VisionRadius,
                Y = _place.Position.Y + Math.Sin(_place.Angle + Math.PI / 3.0) * Statistics.VisionRadius
            };

            return FindCreatureInCircle(visionCenter, Statistics.VisionRadius) != null;
        }
    }
}
