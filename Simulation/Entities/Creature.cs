using System;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Statistics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;
using DawnOnline.Simulation.Senses;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Entities
{
    internal class Creature : IAvatar
    {
        private Placement _place = new Placement();
        private ActionQueue _actionQueue = new ActionQueue();
        private CharacterSheet _characterSheet = new CharacterSheet();
        private AbstractBrain _brain;

        public EntityType Specy { get; set; }
        public EntityType FoodSpecy { get; set; }
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


        private bool _alive = true;


        internal Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
            _place.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, (float)bodyRadius, 1.0f);
            _place.Fixture.Body.BodyType = BodyType.Dynamic;
            _place.Fixture.Body.Mass = 50;
            //_place.Fixture.Friction = 0.1f;
            _place.Fixture.Body.LinearDamping = 1.5f;
            _place.Fixture.Body.AngularDamping = 1f;

            _place.Fixture.UserData = this;
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
            // toSeconds is NOT needed for farseer updates => uses timeDelta in own update
            double toSeconds = timeDelta / 1000.0;

            _characterSheet.Damage.Increase((int)_actionQueue.Damage);
            _actionQueue.Damage = 0;

            if (_characterSheet.Damage.IsFilled)
            {
                MyEnvironment.KillCreature(this);
                return;
            }


            // Move
            _place.Fixture.Body.ApplyForce(_actionQueue.ForwardMotion + _actionQueue.StrafeMotion);
            //_place.Fixture.Body.Rotation += (float)(_actionQueue.TurnMotion * toSeconds);
            //_place.Fixture.Body.ApplyAngularImpulse(((float)(_actionQueue.TurnMotion)));
            _place.Fixture.Body.AngularVelocity = (float)_actionQueue.TurnMotion;

            // Fatigue
            CharacterSheet.Fatigue.Increase((int)(_actionQueue.FatigueCost * toSeconds));

            // Fire
            if (_actionQueue.Fire)
            {
                var bulletAngleVector = new Vector2((float)Math.Cos(_place.Angle), (float)Math.Sin(_place.Angle));
                var bullet = BulletBuilder.CreateBullet(CharacterSheet.RangeDamage);
                //bullet.Launch(bulletAngleVector);
                {
                    MyEnvironment.AddBullet(bullet, _place.Fixture.Body.Position + bulletAngleVector * 30);
                    bullet.Placement.Fixture.Body.ApplyLinearImpulse(bulletAngleVector * 300);
                }

                _actionQueue.HasFired = true;
                _actionQueue.Fire = false;
            }

            // Fire rocket
            if (_actionQueue.FireRocket)
            {
                var bulletAngleVector = new Vector2((float)Math.Cos(_place.Angle), (float)Math.Sin(_place.Angle));
                var bullet = BulletBuilder.CreateRocket(CharacterSheet.RangeDamage);
                //bullet.Launch(bulletAngleVector);
                {
                    MyEnvironment.AddBullet(bullet, _place.Fixture.Body.Position + bulletAngleVector * 30);
                    bullet.Placement.Fixture.Body.ApplyLinearImpulse(bulletAngleVector * 200);
                }

                _actionQueue.HasFired = true;
                _actionQueue.FireRocket = false;
            }


            if (_actionQueue.HasAttacked || _actionQueue.HasFired)
            {
                _actionQueue.LastAttackTime = DateTime.Now;
            }
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            _actionQueue.ForwardMotion = new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                (float)(Math.Sin(_place.Angle) * CharacterSheet.WalkingDistance));
            _actionQueue.FatigueCost = 0;
        }

        public void WalkBackward()
        {
            Debug.Assert(MyEnvironment != null);

            _actionQueue.ForwardMotion = new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                (float)(Math.Sin(_place.Angle) * CharacterSheet.WalkingDistance)) * -0.5f;
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

            _actionQueue.ForwardMotion = new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.RunningDistance),
                                                 (float)(Math.Sin(_place.Angle)*CharacterSheet.RunningDistance));

            _actionQueue.FatigueCost = CharacterSheet.FatigueCost;
        }

        public void Fire()
        {
            if (!CanAttack())
                return;

            _actionQueue.Fire = true;
        }

        public void FireRocket()
        {
            if (!CanAttack())
                return;

            _actionQueue.FireRocket = true;
        }

        internal void Take(Collectable collectable)
        {
            // TODO: score!
            Environment.GetWorld().RemoveObstacle(collectable);
        }

        public void TurnLeft()
        {
            _actionQueue.TurnMotion = -CharacterSheet.TurningAngle;
        }

        public void TurnRight()
        {
            _actionQueue.TurnMotion = CharacterSheet.TurningAngle;
        }

        public void TurnLeftSlow()
        {
            _actionQueue.TurnMotion = -CharacterSheet.TurningAngle / 3.0;
        }

        public void TurnRightSlow()
        {
            _actionQueue.TurnMotion = CharacterSheet.TurningAngle / 3.0;
        }

        public void StrafeLeft()
        {
            _actionQueue.StrafeMotion = new Vector2((float)(Math.Cos(_place.Angle - MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f),
                                               (float)(Math.Sin(_place.Angle - MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f));
        }

        public void StrafeRight()
        {
            _actionQueue.StrafeMotion = new Vector2((float)(Math.Cos(_place.Angle + MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f),
                                               (float)(Math.Sin(_place.Angle + MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f));
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
            Brain.ClearState();

            // TESTING: 
            {
                _characterSheet.Hunger.Increase(necessaryFood);
            }
        }

        internal Creature FindCreatureToAttack(EntityType ofType)
        {
            var attackMiddle = new Vector2(
                (float)(Place.Position.X + Math.Cos(Place.Angle) * CharacterSheet.MeleeRange),
                (float)(Place.Position.Y + Math.Sin(Place.Angle) * CharacterSheet.MeleeRange));

            var creaturesToAttack = ofType == EntityType.Unknown ?  
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
            if (!Alive)
                return false;
            return ((DateTime.Now - _actionQueue.LastAttackTime).TotalSeconds > CharacterSheet.CoolDown);
        }

        public void Attack()
        {
            if (!CanAttack())
                return;

            var creatureToAttack = FindCreatureToAttack(EntityType.Unknown);
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
            this.MyActionQueue.Damage += bullet.Damage;
        }

        internal void TakeExplosionDamage(Bullet bullet, double distance)
        {
            var rangeMinusDistance = Math.Max(Math.Abs(bullet.Range - distance), 0);
            this.MyActionQueue.Damage += bullet.Damage * rangeMinusDistance * rangeMinusDistance / bullet.Range / bullet.Range;
        }

        public bool TryReproduce()
        {
            return false;
            if (!CharacterSheet.Reproduction.IsFilled)
                return false;


            Creature child = CreatureBuilder.CreateCreature(Specy) as Creature;

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
