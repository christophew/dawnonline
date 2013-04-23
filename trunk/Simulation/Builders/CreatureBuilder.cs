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

        public static ICreature CreateCreature(EntityType specy, IEntity spawnPoint, IBrain brain)
        {
            var creature = CreateCreature(specy, brain) as Creature;
            Debug.Assert(creature != null);
            creature.SpawnPoint = spawnPoint;
            return creature;
        }

        public static ICreature CreateCreature(EntityType specy, IBrain brain)
        {
            switch (specy)
            {
                case EntityType.Avatar:
                    return CreateAvatar();
                case EntityType.Plant:
                    return CreatePlant(brain);
                case EntityType.Predator:
                    return CreatePredator(brain);
                case EntityType.Predator2:
                    return CreatePredator2(brain);
                case EntityType.Rabbit:
                    return CreateRabbit(brain);
                case EntityType.Turret:
                    return CreateTurret(brain);
                case EntityType.PredatorSpawnPoint:
                    return CreateSpawnPoint(brain);
                case EntityType.PredatorSpawnPoint2:
                    return CreateSpawnPoint2(brain);
                case EntityType.RabbitSpawnPoint:
                    return CreateRabbitSpawnPoint(brain);
            }

            throw new NotSupportedException();
        }
        
        public static ICreature CreatePredator(IBrain brain)
        {
            var critter = new Creature(1.5);
            critter.Brain = brain;

            critter.Specy = EntityType.Predator;
            critter.FoodSpecies = new List<EntityType> { EntityType.Predator2, EntityType.PredatorSpawnPoint2, EntityType.Rabbit, EntityType.RabbitSpawnPoint };

            critter.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.5 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 10;
            critter.CharacterSheet.MeleeRange = 2.0;
            critter.CharacterSheet.RangeDamage = 0;
            critter.CharacterSheet.UseAutoAttack = true;

            return critter;
        }

        public static ICreature CreatePredator2(IBrain brain)
        {
            var critter = new Creature(2.5);
            critter.Brain = brain;

            critter.Specy = EntityType.Predator2;
            critter.FoodSpecies = new List<EntityType> { EntityType.Predator, EntityType.PredatorSpawnPoint, EntityType.Rabbit, EntityType.RabbitSpawnPoint };

            critter.CharacterSheet.WalkingDistance = 20 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.0 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 20;
            critter.CharacterSheet.MeleeRange = 3;
            critter.CharacterSheet.RangeDamage = 0;
            critter.CharacterSheet.UseAutoAttack = true;

            critter.CharacterSheet.Armour = 2;

            return critter;
        }

        public static ICreature CreateRabbit(IBrain brain)
        {
            var critter = new Creature(1);
            critter.Brain = brain;

            critter.Specy = EntityType.Rabbit;
            critter.FoodSpecies = new List<EntityType> { EntityType.Plant };

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 300);
            critter.CharacterSheet.WalkingDistance = 35 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 2 * _turnMultiplier;
            critter.CharacterSheet.MeleeDamage = 10;
            critter.CharacterSheet.MeleeRange = 1.5;
            critter.CharacterSheet.UseAutoAttack = true;
            critter.CharacterSheet.FoodValue = 10;

            return critter;
        }

        public static ICreature CreatePlant(IBrain brain)
        {
            var critter = new Creature(0.5);
            critter.Brain = brain;

            critter.Specy = EntityType.Plant;
            //critter.FoodSpecy = CreatureType.Predator; // instead: killing creatures can produce plants

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(50, 200);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0;
            critter.CharacterSheet.FoodValue = 5;
            critter.CharacterSheet.IsRooted = true;

            return critter;
        }

        public static ICreature CreateAvatar()
        {
            var avatar = new Creature(1.5);

            avatar.Specy = EntityType.Avatar;
            avatar.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            avatar.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
            avatar.CharacterSheet.RangeDamage = 50;
            avatar.CharacterSheet.MeleeDamage = 50;
            avatar.CharacterSheet.Armour = 100; // I'm invincible!

            return avatar;
        }

        public static ICreature CreateTurret(IBrain brain)
        {
            return CreateTurret(EntityType.Avatar, brain);
        }

        public static ICreature CreateTurret(EntityType enemy, IBrain brain)
        {
            var critter = new Creature(1.5);

            critter.Specy = EntityType.Turret;
            critter.FoodSpecies = new List<EntityType> { enemy };

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
            var spawnPoint = new Creature(1.0);

            spawnPoint.Specy = EntityType.PredatorSpawnPoint;
            spawnPoint.Brain = brain;
            spawnPoint.IsSpawnPoint = true;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 5;
            //spawnPoint.CharacterSheet.Armour = 100;

            return spawnPoint;
        }

        public static ICreature CreateSpawnPoint2(IBrain brain)
        {
            var spawnPoint = new Creature(2.0);

            spawnPoint.Specy = EntityType.PredatorSpawnPoint2;
            spawnPoint.Brain = brain;
            spawnPoint.IsSpawnPoint = true;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 5;
            //spawnPoint.CharacterSheet.Armour = 100;

            return spawnPoint;
        }

        public static ICreature CreateRabbitSpawnPoint(IBrain brain)
        {
            var spawnPoint = new Creature(0.5);

            spawnPoint.Specy = EntityType.RabbitSpawnPoint;
            spawnPoint.Brain = brain;
            spawnPoint.IsSpawnPoint = true;

            // Make the spawnPoint part of the family
            spawnPoint.SpawnPoint = spawnPoint;

            spawnPoint.CharacterSheet.FatigueRecovery = 25;
            spawnPoint.CharacterSheet.Armour = 3;
            //spawnPoint.CharacterSheet.Armour = 100;

            return spawnPoint;
        }
    }
}
