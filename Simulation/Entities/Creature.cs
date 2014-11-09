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
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    internal abstract class Creature : ICreature
    {
        public CreatureTypeEnum CreatureType { get; internal set; }

        private int _id = Globals.GenerateUniqueId();
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        protected Placement _place = new Placement();
        protected ActionQueue _actionQueue = new ActionQueue();
        protected CharacterSheet _characterSheet = new CharacterSheet();
        private IBrain _brain;

        public EntityTypeEnum EntityType { get; internal set; }
        public List<CreatureTypeEnum> FoodSpecies { get; internal set; }
        //public int Age { get; private set; }

        public Placement Place { get { return _place; } }
        public CharacterSheet CharacterSheet { get { return _characterSheet; } }

        public IEntity SpawnPoint { get; internal set; }
        public int FamilyId
        {
            get
            {
                var family = SpawnPoint ?? this;
                return family.Id;
            }
        }

        public void SetSpawnPoint(ICreature spawnPoint)
        {
            SpawnPoint = spawnPoint;
        }

        // Is this the correct place?
        public bool IsSpawnPoint { get { return EntityType == EntityTypeEnum.SpawnPoint; } }

        
        public Environment MyEnvironment { get; internal set; }
        public ActionQueue MyActionQueue { get { return _actionQueue; } }

        internal DateTime LatestThinkTime { get; set; }

        public bool Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }


        public bool HasBrain { get { return Brain != null; } }

        public IBrain Brain 
        { 
            get { return _brain; }
            internal set
            {
                _brain = value;
                _brain.SetCreature(this);
            }
        }


        private bool _alive = true;



        internal Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
            _place.Radius = bodyRadius;
        }

        internal void AddToPhysicsEngine()
        {
            var bodyRadius = _place.Form.BoundingCircleRadius;
            _place.Fixture = BodyFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, (float)bodyRadius, 1.0f).FixtureList[0];
            _place.Fixture.Body.BodyType = this.CharacterSheet.IsRooted ? BodyType.Static : BodyType.Dynamic;
            _place.Fixture.Body.Mass = 5;
            //_place.Fixture.Friction = 0.1f;
            _place.Fixture.Body.LinearDamping = 2.5f;
            _place.Fixture.Body.AngularDamping = 2f;

            _place.Fixture.UserData = this;

            if (_brain != null)
            {
                _brain.InitializeSenses();
            }


            // TODO: I really don't like these kind of override constructions
            PrepareCollision();
        }

        public ICreature Replicate(ICreature mate)
        {
            //var newCreature = new Creature(_place.Form.BoundingCircleRadius);
            var newCreature = CreateCreature(_place.Form.BoundingCircleRadius);
            newCreature._characterSheet = CharacterSheet.Replicate();

            //newCreature.Place.Fixture.Body.BodyType = _place.Fixture.Body.BodyType;

            newCreature.EntityType = EntityType;
            newCreature.CreatureType = CreatureType;
            if (FoodSpecies != null)
                newCreature.FoodSpecies = new List<CreatureTypeEnum>(FoodSpecies);

            newCreature.SpawnPoint = IsSpawnPoint ? newCreature : SpawnPoint;

            // crossover
            var creatureMate = mate as Creature;
            Debug.Assert(creatureMate != null, "sodomy!");

            newCreature.Brain = _brain.Replicate(creatureMate._brain);

            return newCreature;
        }

        public void Mutate()
        {
            Brain.Mutate();
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

        public void Rest()
        {
            _actionQueue.Rest = true;
        }

        public void RegisterSpawn()
        {
            _actionQueue.RegisterSpawn = true;
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



        internal bool TryToEat(Food collectable)
        {
            // Check if we can eat it
            if (FoodSpecies == null || !FoodSpecies.Contains(collectable.CreatureType))
                return false;


            // TODO: score!
            Environment.GetWorld().RemoveObstacle(collectable);

            // + health
            _characterSheet.Damage.Decrease(collectable.FoodValue);

            // + resource
            _characterSheet.Resource.Increase(collectable.FoodValue);

            // score
            if (SpawnPoint != null)
            {
                var spawnPointCreature = SpawnPoint as Creature;
                Debug.Assert(spawnPointCreature != null);

                spawnPointCreature.CharacterSheet.Statistics.ResourcesGathered += collectable.FoodValue;
            }

            return true;
        }

        public ICreature FindCreatureToAttack(List<CreatureTypeEnum> ofTypes)
        {
            var attackMiddle = new Vector2(
                (float)(Place.Position.X + Math.Cos(Place.Angle) * CharacterSheet.MeleeRange),
                (float)(Place.Position.Y + Math.Sin(Place.Angle) * CharacterSheet.MeleeRange));

            var creaturesToAttack = ofTypes == null ?  
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange) : 
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange, ofTypes);

            foreach (var current in creaturesToAttack)
            {
                // Don't attack my family, everything else is game
                if (this.SpawnPoint != null &&
                    this.SpawnPoint == current.SpawnPoint)
                    continue;

                if (!current.Equals(this))
                    return current;
            }
            return null;
        }


        public bool CanAttack()
        {
            if (!Alive)
                return false;
            return ((DateTime.Now - _actionQueue.LastAttackTime).TotalSeconds > CharacterSheet.AttackCoolDown);
        }

        public void Attack()
        {
            _actionQueue.Attack = true;

            //if (!CanAttack())
            //    return;

            //var creatureToAttack = FindCreatureToAttack(null);
            //if (creatureToAttack == null)
            //    return;

            //Attack(creatureToAttack);
        }







        // TODO: SPLIT ACTIONQUEUE? (client/server queue's have different meaning & operations)

        public void Turn(double percent)
        {
            // TEMP
            _actionQueue.TurnPercent += percent;

            _actionQueue.TurnMotion += CharacterSheet.TurningAngle * percent;
        }

        public void Thrust(double percent)
        {
            // TEMP
            _actionQueue.ForwardThrustPercent += percent;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.RunningDistance),
                                                      (float)(Math.Sin(_place.Angle) * CharacterSheet.RunningDistance)) * (float)percent;

            // TODO: fatigue cost
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            // TEMP
            _actionQueue.ForwardThrustPercent += 0.3;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                      (float)(Math.Sin(_place.Angle) * CharacterSheet.WalkingDistance));
            _actionQueue.FatigueCost = 0;
        }

        public void WalkBackward()
        {
            Debug.Assert(MyEnvironment != null);

            // TEMP
            _actionQueue.ForwardThrustPercent -= 0.3;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
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

            // TEMP
            _actionQueue.ForwardThrustPercent += 1.0;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.RunningDistance),
                                                      (float)(Math.Sin(_place.Angle) * CharacterSheet.RunningDistance));

            //_actionQueue.FatigueCost += CharacterSheet.FatigueCost;
        }

        public void RunBackward()
        {
            Debug.Assert(MyEnvironment != null);

            if (IsTired)
            {
                WalkBackward();
                return;
            }

            // TEMP
            _actionQueue.ForwardThrustPercent -= 1.0;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.RunningDistance),
                                                      (float)(Math.Sin(_place.Angle) * CharacterSheet.RunningDistance)) * -1f;

            //_actionQueue.FatigueCost += CharacterSheet.FatigueCost;
        }

        public void Fire()
        {
            if (!CanAttack())
                return;

            _actionQueue.Fire = true;
            _actionQueue.FatigueCost += CharacterSheet.FatigueCost;
        }

        public void FireRocket()
        {
            if (!CanAttack())
                return;

            _actionQueue.FireRocket = true;
            _actionQueue.FatigueCost += CharacterSheet.FatigueCost * 2;
        }

        public void TurnLeft()
        {
            // TEMP
            _actionQueue.TurnPercent -= 1;

            _actionQueue.TurnMotion += -CharacterSheet.TurningAngle;
        }

        public void TurnRight()
        {
            // TEMP
            _actionQueue.TurnPercent += 1;

            _actionQueue.TurnMotion += CharacterSheet.TurningAngle;
        }

        public void TurnLeftSlow()
        {
            // TEMP
            _actionQueue.TurnPercent -= 0.3;

            _actionQueue.TurnMotion += -CharacterSheet.TurningAngle / 3.0;
        }

        public void TurnRightSlow()
        {
            // TEMP
            _actionQueue.TurnPercent += 0.3;

            _actionQueue.TurnMotion += CharacterSheet.TurningAngle / 3.0;
        }

        public void StrafeLeft()
        {
            _actionQueue.StrafeMotion += new Vector2((float)(Math.Cos(_place.Angle - MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f),
                                               (float)(Math.Sin(_place.Angle - MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f));
        }

        public void StrafeRight()
        {
            _actionQueue.StrafeMotion += new Vector2((float)(Math.Cos(_place.Angle + MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f),
                                               (float)(Math.Sin(_place.Angle + MathHelper.PiOver2) * CharacterSheet.WalkingDistance / 2f));
        }

        public void BuildEntity(EntityTypeEnum entityType)
        {
            if ((DateTime.Now - _actionQueue.LastBuildTime).TotalSeconds > CharacterSheet.BuildCoolDown)
            {
                _actionQueue.BuildEntityOfType = entityType;
            }
        }

        public void SayA(double volume)
        {
            _actionQueue.SpeachVolumeA += volume;
        }

        public void SayB(double volume)
        {
            _actionQueue.SpeachVolumeB += volume;
        }



        #region abstracts

        internal abstract Creature CreateCreature(double radius);

        #endregion

        #region Server only

        protected virtual void PrepareCollision()
        { }

        public virtual void Update(double timeDelta)
        {
            throw new NotImplementedException();
        }

        internal virtual void TakeBulletDamage(Bullet bullet)
        {
            throw new NotImplementedException();
        }

        internal virtual void TakeExplosionDamage(Bullet bullet, double distance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Client only

        public virtual void Think(TimeSpan timeDelta)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
