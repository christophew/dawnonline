using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace DawnOnline.Simulation.Builders
{
    public static class CreatureBuilder
    {
        private const double _velocityMultiplier = 200;
        //private const double _turnMultiplier = 20000;
        private const double _turnMultiplier = 3;

        public static ICreature CreateCreature(EntityType specy)
        {
            switch (specy)
            {
                case EntityType.Plant:
                    return CreatePlant();
                case EntityType.Predator:
                    return CreatePredator();
                case EntityType.Rabbit:
                    return CreateRabbit();
                case EntityType.Turret:
                    return CreateTurret();
            }

            throw new ArgumentOutOfRangeException();
        }

        public static ICreature CreatePredator()
        {
            var critter = new Creature(15);
            critter.Brain = new PredatorBrain();

            critter.Specy = EntityType.Predator;
            critter.FoodSpecies = new List<EntityType> { EntityType.Turret, EntityType.Avatar };

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 150);
            critter.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.5 * _turnMultiplier;
            critter.CharacterSheet.ReproductionIncreaseAverage = 2;
            critter.CharacterSheet.MeleeDamage = 1;
            critter.CharacterSheet.RangeDamage = 0;
            critter.Brain.InitializeSenses();

            return critter;
        }

        public static ICreature CreateRabbit()
        {
            var critter = new Creature(10);
            critter.Brain = new RabbitBrain();

            critter.Specy = EntityType.Rabbit;
            critter.FoodSpecies = new List<EntityType> { EntityType.Plant };

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 300);
            critter.CharacterSheet.WalkingDistance = 15 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.5 * _turnMultiplier;
            critter.CharacterSheet.FoodValue = 500;
            critter.CharacterSheet.ReproductionIncreaseAverage = 7;
            critter.Brain.InitializeSenses();

            return critter;
        }

        public static ICreature CreatePlant()
        {
            var critter = new Creature(12);
            critter.Brain = new PlantBrain();

            critter.Specy = EntityType.Plant;
            //critter.FoodSpecy = CreatureType.Predator; // instead: killing creatures can produce plants

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(50, 200);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0;
            critter.CharacterSheet.FoodValue = 200;
            critter.CharacterSheet.ReproductionIncreaseAverage = 7;
            critter.Brain.InitializeSenses();

            return critter;
        }

        public static IAvatar CreateAvatar()
        {
            var avatar = new Creature(15);

            avatar.Specy = EntityType.Avatar;
            avatar.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            avatar.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
            avatar.CharacterSheet.RangeDamage = 50;
            avatar.CharacterSheet.MeleeDamage = 50;

            return avatar;
        }

        public static ICreature CreateTurret()
        {
            var critter = new Creature(15);

            critter.Specy = EntityType.Turret;
            critter.FoodSpecies = new List<EntityType> { EntityType.Predator };

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 150);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0.5 * _turnMultiplier;
            critter.CharacterSheet.AttackCoolDown = 0.3;
            critter.CharacterSheet.RangeDamage = 1;
            critter.CharacterSheet.MeleeDamage = 0;

            critter.CharacterSheet.VisionDistance = 350;

            critter.Brain = new TurretBrain();
            critter.Brain.InitializeSenses();


            return critter;
        }
    }
}
