using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.Simulation.Builders
{
    public static class CreatureBuilder
    {
        private const double _velocityMultiplier = 5;
        //private const double _turnMultiplier = 20000;
        private const double _turnMultiplier = 2;


        public enum ClientServerMode
        {
            Unknown,
            Client,
            Server
        };


        private static ClientServerMode _clientServerMode = ClientServerMode.Unknown;

        public static void SetClientServerMode(ClientServerMode mode)
        {
            _clientServerMode = mode;
        }

        // Creature FactoryMethod => could become abstract factory
        private static Creature NewCreature(double bodyRadius)
        {
            if (_clientServerMode == ClientServerMode.Client)
                return new CreatureOnClient(bodyRadius);
            if (_clientServerMode == ClientServerMode.Server)
                return new CreatureOnServer(bodyRadius);

            throw new InvalidOperationException("ClientServerMode not set");
        }

        public static ICreature CreateCreature(EntityTypeEnum entityType, CreatureTypeEnum creatureType, IEntity spawnPoint, IBrain brain)
        {
            var creature = CreateCreature(entityType, creatureType, brain) as Creature;
            Debug.Assert(creature != null);
            creature.SpawnPoint = spawnPoint;
            return creature;
        }

        public static ICreature CreateCreature(EntityTypeEnum entityType, CreatureTypeEnum creatureType, IBrain brain)
        {
            switch (entityType)
            {
                case EntityTypeEnum.Creature:
                    switch (creatureType)
                    {
                        case CreatureTypeEnum.Avatar:
                            return CreateAvatar();
                        case CreatureTypeEnum.Plant:
                            return CreatePlant();
                        case CreatureTypeEnum.Plant2:
                            return CreatePlant2();
                        case CreatureTypeEnum.Predator:
                            return CreatePredator(brain);
                        case CreatureTypeEnum.Predator2:
                            return CreatePredator2(brain);
                        case CreatureTypeEnum.Rabbit:
                            return CreateRabbit(brain);
                        case CreatureTypeEnum.Turret:
                            return CreateTurret(brain);
                    }
                    break;
                case EntityTypeEnum.SpawnPoint:
                    switch (creatureType)
                    {
                        case CreatureTypeEnum.Predator:
                            return CreateSpawnPoint(brain);
                        case CreatureTypeEnum.Predator2:
                            return CreateSpawnPoint2(brain);
                        case CreatureTypeEnum.Rabbit:
                            return CreateRabbitSpawnPoint(brain);
                    }
                    break;
            }

            throw new NotSupportedException();
        }
        
        public static ICreature CreatePredator(IBrain brain)
        {
            var critter = NewCreature(1.5);
            critter.Brain = brain;

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Predator;
            critter.FoodSpecies = new List<CreatureTypeEnum> { CreatureTypeEnum.Predator2, CreatureTypeEnum.Rabbit };

            critter.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.5 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 15;
            critter.CharacterSheet.MeleeRange = 2.0;
            critter.CharacterSheet.RangeDamage = 0;
            critter.CharacterSheet.UseAutoAttack = true;
            critter.CharacterSheet.FoodValue = 15;

            return critter;
        }

        public static ICreature CreatePredator2(IBrain brain)
        {
            var critter = NewCreature(2.5);
            critter.Brain = brain;

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Predator2;
            critter.FoodSpecies = new List<CreatureTypeEnum> { CreatureTypeEnum.Predator, CreatureTypeEnum.Rabbit };

            critter.CharacterSheet.WalkingDistance = 20 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.0 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 10;
            critter.CharacterSheet.MeleeRange = 3;
            critter.CharacterSheet.RangeDamage = 0;
            critter.CharacterSheet.UseAutoAttack = true;
            critter.CharacterSheet.FoodValue = 20;

            critter.CharacterSheet.Armour = 2;

            return critter;
        }

        public static ICreature CreateRabbit(IBrain brain)
        {
            var critter = NewCreature(1);
            critter.Brain = brain;

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Rabbit;
            critter.FoodSpecies = new List<CreatureTypeEnum> { CreatureTypeEnum.Plant, CreatureTypeEnum.Plant2 };

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 300);
            critter.CharacterSheet.WalkingDistance = 35 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 2 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 10;
            critter.CharacterSheet.MeleeRange = 1.5;
            critter.CharacterSheet.UseAutoAttack = true;
            critter.CharacterSheet.FoodValue = 10;

            return critter;
        }

        public static ICreature CreatePlant()
        {
            var critter = NewCreature(0.5);
            critter.Brain = new DummyBrain();

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Plant;
            //critter.FoodSpecy = CreatureType.Predator; // instead: killing creatures can produce plants

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(50, 200);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0;
            critter.CharacterSheet.FoodValue = 10;
            critter.CharacterSheet.IsRooted = true;

            critter.CharacterSheet.CanSpawnSeed = true;
            critter.CharacterSheet.CanAutoResourceGather = true;
            critter.CharacterSheet.AutoResourceGatherValue = 1;
            critter.CharacterSheet.AutoResourceGatherCoolDown = 5;


            return critter;
        }

        public static ICreature CreatePlant2()
        {
            var critter = NewCreature(0.25);
            critter.Brain = new DummyBrain();

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Plant2;
            //critter.FoodSpecy = CreatureType.Predator; // instead: killing creatures can produce plants

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(50, 200);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0;
            critter.CharacterSheet.FoodValue = 20;
            critter.CharacterSheet.IsRooted = true;

            critter.CharacterSheet.CanSpawnSeed = true;
            critter.CharacterSheet.CanAutoResourceGather = true;
            critter.CharacterSheet.AutoResourceGatherValue = 1;
            critter.CharacterSheet.AutoResourceGatherCoolDown = 5;


            return critter;
        }

        public static ICreature CreateAvatar()
        {
            var avatar = NewCreature(1.5);

            avatar.FoodSpecies = new List<CreatureTypeEnum> { CreatureTypeEnum.Rabbit, CreatureTypeEnum.Predator, CreatureTypeEnum.Predator2 };
            
            avatar.EntityType = EntityTypeEnum.Creature;
            avatar.CreatureType = CreatureTypeEnum.Avatar;
            avatar.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            avatar.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
            avatar.CharacterSheet.RangeDamage = 50;
            avatar.CharacterSheet.MeleeDamage = 50;
            avatar.CharacterSheet.Armour = 100; // I'm invincible!

            return avatar;
        }

        public static ICreature CreateTurret(IBrain brain)
        {
            return CreateTurret(CreatureTypeEnum.Avatar, brain);
        }

        public static ICreature CreateTurret(CreatureTypeEnum enemy, IBrain brain)
        {
            var critter = NewCreature(1.5);

            critter.EntityType = EntityTypeEnum.Creature;
            critter.CreatureType = CreatureTypeEnum.Turret;
            critter.FoodSpecies = new List<CreatureTypeEnum> { enemy };

            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
            critter.CharacterSheet.AttackCoolDown = 0.3;
            critter.CharacterSheet.RangeDamage = 10;
            critter.CharacterSheet.MeleeDamage = 0;

            critter.CharacterSheet.VisionDistance = 40;

            // Fixed position
            //critter.Place.Fixture.Body.BodyType = BodyType.Static;
            //critter.Place.Fixture.Body.LinearDamping = 100f;

            critter.Brain = brain;


            return critter;
        }

        public static ICreature CreateSpawnPoint(IBrain brain)
        {
            var spawnPoint = NewCreature(1.0);

            spawnPoint.EntityType = EntityTypeEnum.SpawnPoint;
            spawnPoint.CreatureType = CreatureTypeEnum.Predator;
            spawnPoint.Brain = brain;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 5;
            //spawnPoint.CharacterSheet.Armour = 100;
            spawnPoint.CharacterSheet.FoodValue = 20;

            return spawnPoint;
        }

        public static ICreature CreateSpawnPoint2(IBrain brain)
        {
            var spawnPoint = NewCreature(2.0);

            spawnPoint.EntityType = EntityTypeEnum.SpawnPoint;
            spawnPoint.CreatureType = CreatureTypeEnum.Predator2;
            spawnPoint.Brain = brain;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 5;
            //spawnPoint.CharacterSheet.Armour = 100;
            spawnPoint.CharacterSheet.FoodValue = 30;

            return spawnPoint;
        }

        public static ICreature CreateRabbitSpawnPoint(IBrain brain)
        {
            var spawnPoint = NewCreature(0.5);

            spawnPoint.EntityType = EntityTypeEnum.SpawnPoint;
            spawnPoint.CreatureType = CreatureTypeEnum.Rabbit;
            spawnPoint.Brain = brain;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 3;
            //spawnPoint.CharacterSheet.Armour = 100;
            spawnPoint.CharacterSheet.FoodValue = 15;

            return spawnPoint;
        }
    }
}
