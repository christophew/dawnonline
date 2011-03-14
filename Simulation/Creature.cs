using System;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Statistics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;
using DawnOnline.Simulation.Senses;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    public enum CreatureType
    {
        Unknown,
        Avatar,
        Predator,
        Rabbit,
        Plant
    }

    public class Creature
    {
        private Placement _place = new Placement();
        private ActionQueue _actionQueue = new ActionQueue();
        private CharacterSheet _characterSheet = new CharacterSheet();
        private AbstractBrain _brain;

        public CreatureType Specy { get; set; }
        public CreatureType FoodSpecy { get; set; }
        //public int Age { get; private set; }

        public Placement Place { get { return _place; } }
        public CharacterSheet CharacterSheet { get { return _characterSheet; } }

        internal Environment MyEnvironment { get; set; }
        internal ActionQueue MyActionQueue { get { return _actionQueue; } }

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

        internal IList<IEye> Eyes
        {
            get { return new List<IEye> {_forwardEye, _leftEye, _rightEye}; }
        }

        private bool _alive = true;


        internal Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
            _place.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, (float)bodyRadius, 1.0f);
            _place.Fixture.Body.BodyType = BodyType.Dynamic;
            _place.Fixture.Body.Mass = 50;
            //_place.Fixture.Friction = 0.1f;
            _place.Fixture.Body.LinearDamping = 1f;
            _place.Fixture.Body.AngularDamping = 1f;

            _place.Fixture.UserData = this;
        }

        internal void InitializeSenses()
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
                return CharacterSheet.Fatigue.IsCritical;
            }
        }

        public bool IsDying
        {
            get
            {
                return CharacterSheet.Damage.IsCritical;
            }
        }

        public bool IsHungry
        {
            get
            {
                return CharacterSheet.Hunger.IsCritical;
            }
        }

        public void Rest()
        {
            CharacterSheet.Fatigue.Decrease((int)CharacterSheet.FatigueRecovery);
        }

        public void ClearActionQueue()
        {
            _actionQueue.ClearForRound();
        }

        public bool IsAttacking()
        {
            return MyActionQueue.HasAttacked;
        }

        public bool IsAttacked()
        {
            return MyActionQueue.Damage > 0;
        }

        public void ApplyActionQueue(double timeDelta)
        {
            double toSeconds = timeDelta / 1000.0;

            _characterSheet.Damage.Increase((int)_actionQueue.Damage);
            _actionQueue.Damage = 0;

            if (_characterSheet.Damage.IsFilled)
            {
                MyEnvironment.KillCreature(this);
                return;
            }

            //var realTranslation = TryMove(_actionQueue.ForwardMotion * toSeconds);
            //_place.OffsetPosition(new Vector2(realTranslation), 0.0);
            var force = new Vector2(_actionQueue.ForwardMotion.X, _actionQueue.ForwardMotion.Y);
            _place.Fixture.Body.ApplyForce(force);

            //_place.Angle += _actionQueue.TurnMotion * toSeconds;
            _place.Fixture.Body.Rotation += (float)(_actionQueue.TurnMotion * toSeconds);

            CharacterSheet.Fatigue.Increase((int)(_actionQueue.FatigueCost * toSeconds));

            // Fire
            if (_actionQueue.Fire)
            {
                var bulletAngleVector = new Vector2((float)Math.Cos(_place.Angle), (float)Math.Sin(_place.Angle));
                var bullet = SimulationFactory.CreateBullet(CharacterSheet.MeleeDamage);
                // TODO: move to Bullet
                {
                    MyEnvironment.AddBullet(bullet, _place.Fixture.Body.Position + bulletAngleVector*30);
                    bullet.Placement.Fixture.Body.ApplyLinearImpulse(bulletAngleVector*200);
                    //bullet.Fixture.Body.ApplyForce(bulletAngleVector * 200);
                }
                _actionQueue.HasFired = true;
                _actionQueue.Fire = false;
            }


            if (_actionQueue.HasAttacked || _actionQueue.HasFired)
            {
                _actionQueue.LastAttackTime = DateTime.Now;
            }
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            _actionQueue.ForwardMotion = new Vector((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                (float)(Math.Sin(_place.Angle) * CharacterSheet.WalkingDistance));
            _actionQueue.FatigueCost = 0;
        }

        public void WalkBackward()
        {
            Debug.Assert(MyEnvironment != null);

            _actionQueue.ForwardMotion = new Vector((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                (float)(Math.Sin(_place.Angle) * CharacterSheet.WalkingDistance)) * - 0.5;
            _actionQueue.FatigueCost = 0;
        }

        public void RunForward()
        {
            Debug.Assert(MyEnvironment != null);

            if (IsTired)
            {
                WalkForward();
                return;
            }

            _actionQueue.ForwardMotion = new Vector((float)(Math.Cos(_place.Angle) * CharacterSheet.RunningDistance),
                                                 (float)(Math.Sin(_place.Angle)*CharacterSheet.RunningDistance));

            _actionQueue.FatigueCost = CharacterSheet.FatigueCost;
        }

        public void Fire()
        {
            if (!CanAttack())
                return;

            Debug.Assert(Alive);

            _actionQueue.Fire = true;
        }

        private Vector TryMove(Vector velocity)
        {
            foreach (var obstacle in MyEnvironment.GetObstacles())
            {
                // Bounding circle optimization
                if (!MathTools.CirclesIntersect(Place.Position, Place.Form.BoundingCircleRadius, obstacle.Position, obstacle.Form.BoundingCircleRadius))
                    continue;

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
            _actionQueue.TurnMotion = -CharacterSheet.TurningAngle;
        }

        public void TurnRight()
        {
            _actionQueue.TurnMotion = CharacterSheet.TurningAngle;
        }

        public void Move()
        {
            Debug.Assert(Alive);

            if (!HasBrain)
                return;

            ClearActionQueue();

            //if (Age++ > _characterSheet.MaxAge)
            //{
            //    MyEnvironment.KillCreature(this);
            //    return;
            //}
            CharacterSheet.Reproduction.Increase(Globals.Radomizer.Next(0, CharacterSheet.ReproductionIncreaseAverage * 2));

            int necessaryFood = Globals.Radomizer.Next(3);

            //if (Specy == CreatureType.Plant) necessaryFood = 1;

            //if (!_characterSheet.Hunger.CanIncrease(necessaryFood))
            //{
            //    MyEnvironment.KillCreature(this);
            //    return;
            //}

            Brain.DoSomething();

            // TESTING: 
            {
                _characterSheet.Hunger.Increase(necessaryFood);
            }
        }

        internal Creature FindCreatureToAttack(CreatureType ofType)
        {
            var attackMiddle = new Vector2(
                (float)(Place.Position.X + Math.Cos(Place.Angle) * CharacterSheet.MeleeRange),
                (float)(Place.Position.Y + Math.Sin(Place.Angle) * CharacterSheet.MeleeRange));

            var creaturesToAttack = ofType == CreatureType.Unknown ?  
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange) : 
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange, ofType);

            foreach (Creature current in creaturesToAttack)
            {
                if (!current.Equals(this))
                    return current;
            }
            return null;
        }


        public bool CanAttack()
        {
            return ((DateTime.Now - _actionQueue.LastAttackTime).TotalSeconds > CharacterSheet.MeleeCoolDown);
        }

        public void Attack()
        {
            if (!CanAttack())
                return;

            var creatureToAttack = FindCreatureToAttack(CreatureType.Unknown);
            if (creatureToAttack == null)
                return;

            Attack(creatureToAttack);
        }

        internal void Attack(Creature target)
        {
            if (!CanAttack())
                return;

            Debug.Assert(Alive);

            _actionQueue.HasAttacked = true;
            target.MyActionQueue.Damage = _characterSheet.MeleeDamage;
        }

        internal void TakeBulletDamage(Bullet bullet)
        {
            this.MyActionQueue.Damage = bullet.Damage;
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

        public bool SeesACreatureForward(CreatureType specy)
        {
            return _forwardEye.SeesACreature(specy);
        }

        public bool SeesACreatureLeft(CreatureType specy)
        {
            return _leftEye.SeesACreature(specy);
        }

        public bool SeesACreatureRight(CreatureType specy)
        {
            return _rightEye.SeesACreature(specy);
        }

        public bool TryReproduce()
        {
            return false;
            if (!CharacterSheet.Reproduction.IsFilled)
                return false;


            Creature child = SimulationFactory.CreateCreature(Specy);

            MyEnvironment.AddCreature(
                child,
                MathTools.OffsetCoordinate(
                    _place.Position,
                    _place.Angle + Math.PI,
                    _place.Form.BoundingCircleRadius + 5),
                Globals.Radomizer.Next(7));


            CharacterSheet.Reproduction.Clear();

            return true;
        }


    }
}
