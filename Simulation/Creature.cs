using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simulation
{
    class Creature : ICreature
    {
        private IForm _body = SimulationFactory.CreateCircle(15);
        private double _walkingDistance = 5;
        private double _turningAngle = 0.2;
        private Placement _place = new Placement();
        private bool _alive = true;
        private int _visionAccuracyPercent = 75;
        private double _visionRadius = 40;

        public Creature()
        {
            _place.Form = _body;
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            _place.Position.X += Math.Cos(_place.Angle) * WalkingDistance;
            _place.Position.Y += Math.Sin(_place.Angle) * WalkingDistance;
        }

        public void TurnLeft()
        {
            _place.Angle -= _turningAngle;
        }

        public void TurnRight()
        {
            _place.Angle += _turningAngle;
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

        public double TurningAngle
        {
            get { return _turningAngle; }
            set { _turningAngle = value; }
        }

        public double WalkingDistance
        {
            get { return _walkingDistance; }
            set { _walkingDistance = value; }
        }

        public double VisionRadius
        {
            get { return _visionRadius; }
            set { _visionRadius = value; }
        }

        public bool HasBrain { get; set; }

        public void Move()
        {
            Debug.Assert(Alive);

            if (!HasBrain)
                return;

            if ((Globals.Radomizer.Next(100) < _visionAccuracyPercent) && SeesACreatureForward())
            {
                WalkForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < _visionAccuracyPercent) && SeesACreatureLeft())
            {
                TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < _visionAccuracyPercent) && SeesACreatureRight())
            {
                TurnRight();
                return;
            }

            int randomAction = Globals.Radomizer.Next(3);

            if (randomAction == 0)
                WalkForward();
            if (randomAction == 1)
                TurnLeft();
            if (randomAction == 2)
                TurnRight();
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
            double forwardRadius = _visionRadius;

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
                X = _place.Position.X + Math.Cos(_place.Angle - Math.PI / 3.0) * _visionRadius,
                Y = _place.Position.Y + Math.Sin(_place.Angle - Math.PI / 3.0) * _visionRadius
            };

            return FindCreatureInCircle(visionCenter, _visionRadius) != null;
        }

        public bool SeesACreatureRight()
        {
            var visionCenter = new Coordinate
            {
                X = _place.Position.X + Math.Cos(_place.Angle + Math.PI / 3.0) * _visionRadius,
                Y = _place.Position.Y + Math.Sin(_place.Angle + Math.PI / 3.0) * _visionRadius
            };

            return FindCreatureInCircle(visionCenter, _visionRadius) != null;
        }
    }
}
