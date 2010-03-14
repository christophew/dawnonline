﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public static class SimulationFactory
    {
        private const double _velocityMultiplier = 2;

        public static IEnvironment CreateEnvironment()
        {
            return new Environment();
        }

        public static IForm CreateCircle(double radius)
        {
            float halfRadius = (float)(radius / 2.0);
            Polygon box = new Polygon();
            box.Points.Add(new Vector(halfRadius, halfRadius));
            box.Points.Add(new Vector(-halfRadius, halfRadius));
            box.Points.Add(new Vector(-halfRadius, -halfRadius));
            box.Points.Add(new Vector(halfRadius, -halfRadius));
            box.BuildEdges();

            return new Form { BoundingCircleRadius = radius, Shape = box };
        }

        public static IPlacement CreateObstacleBox(double deltaX, double deltaY)
        {
            double radius = Math.Max(deltaX, deltaY);

            Polygon box = new Polygon();
            float halveDeltaX = (float) (deltaX/2.0);
            float halveDeltaY = (float) (deltaY/2.0);

            box.Points.Add(new Vector(- halveDeltaX, - halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
            box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };

            return placement;
        }

        public static ICreature CreateCreature(CreatureType specy)
        {
            switch (specy)
            {
                case CreatureType.Plant:
                    return CreatePlant();
                case CreatureType.Predator:
                    return CreatePredator();
                case CreatureType.Rabbit:
                    return CreateRabbit();
            }

            throw new ArgumentOutOfRangeException();
        }

        public static ICreature CreatePredator()
        {
            var critter = new Creature(15);
            critter.Brain = new PredatorBrain();

            critter.Specy = CreatureType.Predator;
            critter.FoodSpecy = CreatureType.Avatar;

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 150);
            critter.CharacterSheet.WalkingDistance = 20 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1;
            critter.CharacterSheet.ReproductionIncreaseAverage = 2;
            critter.InitializeSenses();

            return critter;
        }

        public static ICreature CreateRabbit()
        {
            var critter = new Creature(10);
            critter.Brain = new RabbitBrain();

            critter.Specy = CreatureType.Rabbit;
            critter.FoodSpecy = CreatureType.Plant;

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 300);
            critter.CharacterSheet.WalkingDistance = 15 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1.5;
            critter.CharacterSheet.FoodValue = 500;
            critter.CharacterSheet.ReproductionIncreaseAverage = 7;
            critter.InitializeSenses();

            return critter;
        }

        public static ICreature CreatePlant()
        {
            var critter = new Creature(12);
            critter.Brain = new PlantBrain();

            critter.Specy = CreatureType.Plant;
            //critter.FoodSpecy = CreatureType.Predator; // instead: killing creatures can produce plants

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(50, 200);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 0;
            critter.CharacterSheet.FoodValue = 200;
            critter.CharacterSheet.ReproductionIncreaseAverage = 7;
            critter.InitializeSenses();

            return critter;
        }

        public static ICreature CreateAvatar()
        {
            var avatar = new Creature(15);

            avatar.Specy = CreatureType.Avatar;
            avatar.CharacterSheet.WalkingDistance = 30 * _velocityMultiplier;
            avatar.CharacterSheet.TurningAngle = 1;
            avatar.InitializeSenses();

            return avatar;
        }
    }
}
