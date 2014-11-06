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
    internal class Creature : ICreature
    {
        public CreatureTypeEnum CreatureType { get; internal set; }

        private int _id = Globals.GenerateUniqueId();
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private Placement _place = new Placement();
        protected ActionQueue _actionQueue = new ActionQueue();
        private CharacterSheet _characterSheet = new CharacterSheet();
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

        public static bool DeliverOnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // Should only be triggered on the server
            Debug.Assert(Globals.GetInstanceId() == 0, "Should only be triggered on the server");


            var spawnPoint = fixtureA.UserData as Creature;
            Debug.Assert(spawnPoint != null);
            Debug.Assert((spawnPoint.IsSpawnPoint), "Should be bound to a SpawnPoint");

            var deliveryCreature = fixtureB.UserData as Creature;
            if (deliveryCreature == null)
                return true;

            // Only deliver to my own spawnPoint
            if (deliveryCreature.SpawnPoint != spawnPoint)
                return true;


            deliveryCreature.DoDeliverResources(spawnPoint);


            return true;
        }


        internal Creature(double bodyRadius)
        {
            _place.Form = SimulationFactory.CreateCircle(bodyRadius);
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

            // TODO: some better checks
            // TODO: a better place todo this => should be in the CreatureBuilder, but we only have a Fixture after we add it to the Engine
            if ((Globals.GetInstanceId() == 0) && IsSpawnPoint)
            {
                _place.Fixture.OnCollision += Creature.DeliverOnCollision;
            }
        }

        public ICreature Replicate(ICreature mate)
        {
            var newCreature = new Creature(_place.Form.BoundingCircleRadius);
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

        //public ICreature Replicate()
        //{
        //    Debug.Assert(Specy == EntityType.SpawnPoint, "Not implemented for anything else!!");

        //    var newCreature = CreatureBuilder.CreateSpawnPoint(EntityType.Predator) as Creature;
        //    newCreature.Brain = this.Brain.Replicate();
        //    newCreature.Brain.InitializeSenses();
        //    return newCreature;
        //}

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

        private void DoRest()
        {
            if ((DateTime.Now - _actionQueue.LastRestTime).TotalSeconds < CharacterSheet.RestCoolDown)
                return;
            CharacterSheet.Fatigue.Decrease((int)CharacterSheet.FatigueRecovery);
            _actionQueue.LastRestTime = DateTime.Now;
        }

        private void DoRegen()
        {
            if ((DateTime.Now - _actionQueue.LastRegenTime).TotalSeconds < CharacterSheet.RegenCoolDown)
                return;
            CharacterSheet.Fatigue.Decrease((int)CharacterSheet.FatigueRegen);
            CharacterSheet.Damage.Decrease((int)CharacterSheet.HealthRegen);
            _actionQueue.LastRegenTime = DateTime.Now;
        }

        public void RegisterSpawn()
        {
            _actionQueue.RegisterSpawn = true;
        }

        private void DoSpawn()
        {
            // TODO: check against LastSpawnTime

            // Security
            if (!IsSpawnPoint)
                throw new InvalidOperationException();

            // Fatigue
            CharacterSheet.Fatigue.Increase(30);

            // Resource
            CharacterSheet.Resource.Decrease(10);

            // Score
            CharacterSheet.Statistics.NrOfSpawns++;


            // Reset actionQueue => register should only happen once
            _actionQueue.RegisterSpawn = false;
        }

        private void DoDeliverResources(Creature spawnPoint)
        {
            // Only deliver to my own spawnPoint
            Debug.Assert(this.SpawnPoint == spawnPoint);

            // Score
            // TODO refactor
            spawnPoint.CharacterSheet.Score += CharacterSheet.Resource.PercentFilled;

            // Score
            spawnPoint.CharacterSheet.Statistics.ResourcesDelivered += CharacterSheet.Resource.PercentFilled;
            if (CharacterSheet.Resource.PercentFilled > 0.0)
                spawnPoint.CharacterSheet.Statistics.NrOfTimesResourcesDelivered++;

            // Exchange resource
            spawnPoint.CharacterSheet.Resource.Increase((int)CharacterSheet.Resource.PercentFilled);
            CharacterSheet.Resource.Clear();
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

        public virtual void Update(double timeDelta)
        {
            ApplyActionQueue(timeDelta);

            // TO VERIFY: is this the correct place to update the score?
            CharacterSheet.UpdateScore();
        }

        protected void ApplyActionQueue(double timeDelta)
        {
            // toSeconds is NOT needed for farseer updates => uses timeDelta in own update
            double toSeconds = timeDelta / 1000.0;

            var spawnPointCreature = SpawnPoint as Creature;

            int resultingDamage = (int)Math.Max(_actionQueue.Damage - _characterSheet.Armour, 0);
            _characterSheet.Damage.Increase(resultingDamage);
            _actionQueue.Damage = 0;
            if (spawnPointCreature != null)
                spawnPointCreature.CharacterSheet.Statistics.DamageReceived += resultingDamage;

            if (_characterSheet.Damage.IsFilled)
            {
                var position = this.Place.Position;

                if (spawnPointCreature != null)
                    spawnPointCreature.CharacterSheet.Statistics.NrOfOwnCreaturesKilled++;

                MyEnvironment.KillCreature(this);

                // Add treasure where creature is killed
                var treasure = ObstacleBuilder.CreateTreasure(CreatureType, this.CharacterSheet.FoodValue); // Add the gathered resources to the FoodValue as well?
                MyEnvironment.AddObstacle(treasure, position);

                return;
            }


            // Move
            _place.Fixture.Body.ApplyForce(_actionQueue.ForwardMotion + _actionQueue.StrafeMotion);
            _place.Fixture.Body.AngularVelocity = (float)(_actionQueue.TurnMotion);

            // Fatigue
            CharacterSheet.Fatigue.Increase((int)(_actionQueue.FatigueCost * toSeconds));

            // Attack
            if (_actionQueue.Attack || CharacterSheet.UseAutoAttack)
            {
                ApplyAttack();
            }

            // Fire
            if (_actionQueue.Fire)
            {
                var bulletAngleVector = new Vector2((float)Math.Cos(_place.Angle), (float)Math.Sin(_place.Angle));
                var bullet = BulletBuilder.CreateBullet(CharacterSheet.RangeDamage);
                //bullet.Launch(bulletAngleVector);
                {
                    MyEnvironment.AddBullet(
                        bullet, 
                        _place.Fixture.Body.Position + bulletAngleVector * (float)_place.Form.BoundingCircleRadius * 2.0f,
                        _place.Angle);
                    bullet.Place.Fixture.Body.ApplyLinearImpulse(bulletAngleVector * 10);
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
                    MyEnvironment.AddBullet(
                        bullet, 
                        _place.Fixture.Body.Position + bulletAngleVector * (float)_place.Form.BoundingCircleRadius * 2.0f,
                        _place.Angle);
                    //bullet.Place.Fixture.Body.ApplyLinearImpulse(bulletAngleVector * 20);
                    bullet.Place.Fixture.Body.ApplyLinearImpulse(bulletAngleVector * 5);
                }

                _actionQueue.HasFired = true;
                _actionQueue.FireRocket = false;
            }


            if (_actionQueue.HasAttacked || _actionQueue.HasFired)
            {
                _actionQueue.LastAttackTime = DateTime.Now;
            }

            // Build
            if (_actionQueue.BuildEntityOfType != EntityTypeEnum.Unknown)
            {
                DoBuildEntity(_actionQueue.BuildEntityOfType);
            }

            // Rest
            if (_actionQueue.Rest)
            {
                DoRest();
            }

            // Spawn
            if (_actionQueue.RegisterSpawn)
            {
                DoSpawn();
            }

            // Speak
            if (_actionQueue.SpeachVolumeA > 0)
            {
                var sound = SoundBuilder.CreateSoundForCreature(this, Sound.SoundTypeEnum.A, _actionQueue.SpeachVolumeA);
                MyEnvironment.AddSound(sound);
            }
            if (_actionQueue.SpeachVolumeB > 0)
            {
                var sound = SoundBuilder.CreateSoundForCreature(this, Sound.SoundTypeEnum.B, _actionQueue.SpeachVolumeB);
                MyEnvironment.AddSound(sound);
            }

            // Auto monitor adjusts
            DoRegen();
        }

        public void WalkForward()
        {
            Debug.Assert(MyEnvironment != null);

            // TEMP
            _actionQueue.ForwardThrustPercent += 0.3;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                      (float) (Math.Sin(_place.Angle)*CharacterSheet.WalkingDistance));
            _actionQueue.FatigueCost = 0;
        }

        public void WalkBackward()
        {
            Debug.Assert(MyEnvironment != null);

            // TEMP
            _actionQueue.ForwardThrustPercent -= 0.3;

            _actionQueue.ForwardMotion += new Vector2((float)(Math.Cos(_place.Angle) * CharacterSheet.WalkingDistance),
                                                      (float) (Math.Sin(_place.Angle)*CharacterSheet.WalkingDistance))*-0.5f;
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
                                                      (float) (Math.Sin(_place.Angle)*CharacterSheet.RunningDistance));

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
                                                      (float) (Math.Sin(_place.Angle)*CharacterSheet.RunningDistance))*-1f;

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

        public void Think(TimeSpan timeDelta)
        {
            Debug.Assert(Alive);

            if (!HasBrain)
                return;

            // Clear action queue: the brain will select new actions
            ClearActionQueue();

            Brain.DoSomething(timeDelta);
            Brain.ClearState();
        }

        public ICreature FindCreatureToAttack(List<CreatureTypeEnum> ofTypes)
        {
            var attackMiddle = new Vector2(
                (float)(Place.Position.X + Math.Cos(Place.Angle) * CharacterSheet.MeleeRange),
                (float)(Place.Position.Y + Math.Sin(Place.Angle) * CharacterSheet.MeleeRange));

            var creaturesToAttack = ofTypes == null ?  
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange) : 
                MyEnvironment.GetCreaturesInRange(attackMiddle, CharacterSheet.MeleeRange, ofTypes);

            foreach (Creature current in creaturesToAttack)
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

        private void ApplyAttack()
        {
            if (!CanAttack())
                return;

            Debug.Assert(Alive);

            _actionQueue.HasAttacked = true;
            _actionQueue.FatigueCost += CharacterSheet.FatigueCost;

            var target = FindCreatureToAttack(null);
            if (target != null)
            {
                target.MyActionQueue.Damage += _characterSheet.MeleeDamage;

                // Score
                var spawnPoint = SpawnPoint as Creature;
                if (spawnPoint != null)
                {
                    spawnPoint.CharacterSheet.Statistics.DamageDone += _characterSheet.MeleeDamage;
                }
            }
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

        public void BuildEntity(EntityTypeEnum entityType)
        {
            if ((DateTime.Now - _actionQueue.LastBuildTime).TotalSeconds > CharacterSheet.BuildCoolDown)
            {
                _actionQueue.BuildEntityOfType = entityType;
            }
        }

        public void DoBuildEntity(EntityTypeEnum entityType)
        {
            // TODO: inject the prototype into the Creature

            //if (entityType == EntityType.Unknown)
            //{
            //    return;
            //}
            //if (entityType == EntityType.Turret)
            //{
            //    var turret = CreatureBuilder.CreateTurret(EntityType.Predator);
            //    MyEnvironment.AddCreature(turret, Place.Position, Place.Angle);
            //    _actionQueue.LastBuildTime = DateTime.Now;
            //    return;
            //}

            throw new NotSupportedException();
        }

        public void SayA(double volume)
        {
            _actionQueue.SpeachVolumeA += volume;
        }

        public void SayB(double volume)
        {
            _actionQueue.SpeachVolumeB += volume;
        }
    }
}
