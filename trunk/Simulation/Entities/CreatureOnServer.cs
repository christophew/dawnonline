using System;
using System.Diagnostics;
using DawnOnline.Simulation.Builders;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    internal class CreatureOnServer : Creature
    {
        public CreatureOnServer(double bodyRadius)
            : base(bodyRadius)
        {}

        internal override  Creature CreateCreature(double radius)
        {
            return new CreatureOnServer(_place.Radius);
        }

        internal override void TakeBulletDamage(Bullet bullet)
        {
            this.MyActionQueue.Damage += bullet.Damage;
        }

        internal override void TakeExplosionDamage(Bullet bullet, double distance)
        {
            var rangeMinusDistance = Math.Max(Math.Abs(bullet.Range - distance), 0);
            this.MyActionQueue.Damage += bullet.Damage * rangeMinusDistance * rangeMinusDistance / bullet.Range / bullet.Range;
        }


        public override void Update(double timeDelta)
        {
            ApplyActionQueue(timeDelta);

            // TO VERIFY: is this the correct place to update the score?
            //CharacterSheet.UpdateScore();
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

            // PLANT ONLY ACTIONS
            // TODO: add to action queue as wellL (initiated by plant brain?)
            if (CharacterSheet.CanSpawnSeed)
                DoSpawnSeed();
            if (CharacterSheet.CanAutoResourceGather)
                DoAutoResourceGather();

        }

        private void DoSpawnSeed()
        {
            if (CharacterSheet.Resource.PercentFilled > CharacterSheet.FoodValue)
            {
                var position = this.Place.Position;

                //var direction = new Vector3()

                // Add treasure where creature is killed
                var treasure = ObstacleBuilder.CreateTreasure(CreatureType, CharacterSheet.FoodValue);
                var angle = Globals.Radomizer.NextDouble() * MathHelper.TwoPi;
                MyEnvironment.AddObstacle(treasure, position, angle);

                // Move seed
                var maxForce = 200;
                var force = new Vector2((float)(Globals.Radomizer.NextDouble() - 0.5) * maxForce,
                    (float)(Globals.Radomizer.NextDouble() - 0.5) * maxForce);
                treasure.Place.Fixture.Body.ApplyForce(force);

                CharacterSheet.Resource.Decrease((int)CharacterSheet.FoodValue);
            }
        }

        private void DoAutoResourceGather()
        {
            if ((DateTime.Now - _actionQueue.LastAutoResourceGainTime).TotalSeconds < CharacterSheet.AutoResourceGatherCoolDown)
                return;

            var resourcesGathered = MyEnvironment.GatherResources(CharacterSheet.AutoResourceGatherValue);
            CharacterSheet.Resource.Increase(resourcesGathered);

            _actionQueue.LastAutoResourceGainTime = DateTime.Now;
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


        private void DoBuildEntity(EntityTypeEnum entityType)
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

        protected override void PrepareCollision()
        {
            Assert.That(Globals.IsServerInstance());

            // TODO: a better place todo this => should be in the CreatureBuilder, but we only have a Fixture after we add it to the Engine
            if (IsSpawnPoint)
            {
                _place.Fixture.OnCollision += CreatureOnServer.DeliverOnCollision;
            }
        }

        public static bool DeliverOnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // Should only be triggered on the server
            Debug.Assert(Globals.GetInstanceId() == 0, "Should only be triggered on the server");


            var spawnPoint = fixtureA.UserData as CreatureOnServer;
            Debug.Assert(spawnPoint != null);
            Debug.Assert((spawnPoint.IsSpawnPoint), "Should be bound to a SpawnPoint");

            var deliveryCreature = fixtureB.UserData as CreatureOnServer;
            if (deliveryCreature == null)
                return true;

            // Only deliver to my own spawnPoint
            if (deliveryCreature.SpawnPoint != spawnPoint)
                return true;


            deliveryCreature.DoDeliverResources(spawnPoint);


            return true;
        }

    }
}