using System;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Statistics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;
using DawnOnline.Simulation.Senses;

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

        private Eye _forwardEye;
        private Eye _leftEye;
        private Eye _rightEye;

        public IList<IEye> Eyes
        {
            get { return new List<IEye> {_forwardEye, _leftEye, _rightEye}; }
        }

        internal CharacterSheet Statistics { get { return _characterSheet; } }

        internal bool _alive = true;


        public Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
        }

        public void InitializeSenses()
        {
            _forwardEye = new Eye(this)
            {
                Angle = 0.0,
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = _characterSheet.VisionDistance
            };
            _leftEye = new Eye(this)
            {
                Angle = -MathTools.ConvertToRadials(60),
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = _characterSheet.VisionDistance
            };
            _rightEye = new Eye(this)
            {
                Angle = MathTools.ConvertToRadials(60),
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = _characterSheet.VisionDistance
            };
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
            _place.OffsetPosition(new Coordinate(realTranslation), 0.0);
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
            _place.OffsetPosition(new Coordinate(realTranslation), 0.0);

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
                    velocity = velocity + collitionResult.MinimumTranslationVector;
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

            return FindCreatureInCircle(_place.Position, _characterSheet.MeleeRange);
        }

        private static bool IsInCircle(Creature enemy, Coordinate position, double radius)
        {
            return MathTools.GetDistance2(position, enemy.Place.Position) < radius * radius;
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
            return _forwardEye.SeesACreature();
        }

        public bool SeesACreatureLeft()
        {
            return _leftEye.SeesACreature();
        }

        public bool SeesACreatureRight()
        {
            return _rightEye.SeesACreature();
        }
    }
}
