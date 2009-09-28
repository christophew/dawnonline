using System;
using System.Diagnostics;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Statistics;

namespace DawnOnline.Simulation
{
    class Creature : ICreature
    {
        private IForm _body; 
        private Placement _place = new Placement();
        private CharacterSheet _characterSheet = new CharacterSheet();
        private AbstractBrain _brain;

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
            _body = SimulationFactory.CreateCircle(bodyRadius);
            _place.Form = _body;
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

            _place.Position.X += Math.Cos(_place.Angle) * Statistics.WalkingDistance;
            _place.Position.Y += Math.Sin(_place.Angle) * Statistics.WalkingDistance;
        }

        public void RunForward()
        {
            Debug.Assert(MyEnvironment != null);

            if (IsTired)
            {
                WalkForward();
                return;
            }

            _place.Position.X += Math.Cos(_place.Angle) * Statistics.RunningDistance;
            _place.Position.Y += Math.Sin(_place.Angle) * Statistics.RunningDistance;

            Statistics.Fatigue.Increase((int)Statistics.FatigueCost);
        }

        public void TurnLeft()
        {
            _place.Angle -= Statistics.TurningAngle;
        }

        public void TurnRight()
        {
            _place.Angle += Statistics.TurningAngle;
        }

        public void SetPosition(Coordinate position, double angle)
        {
            _place.Position = position;
            _place.Angle = angle;
        }

        public Environment MyEnvironment { get; set; }
        public IForm Body { get { return _body; } }
        public IPlacement Place { get { return _place; } }

        public bool Alive
        { 
            get { return _alive; }
            set { _alive = value; }
        }

        public bool HasBrain { get { return Brain != null; } }

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

            return FindCreatureInCircle(_place.Position, _body.Radius);
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
