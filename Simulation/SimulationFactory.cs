using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace DawnOnline.Simulation
{
    public static class SimulationFactory
    {
        //private const double _velocityMultiplier = 5;
        private const double _velocityMultiplier = 200;
        private const double _turnMultiplier = 4;

        public static Environment CreateEnvironment()
        {
            return Environment.GetWorld();
        }

        public static Form CreateCircle(double radius)
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

        public static IEntity CreateObstacleBox(double deltaX, double deltaY)
        {
            double radius = Math.Max(deltaX, deltaY);

            Polygon box = new Polygon();
            float halveDeltaX = (float)(deltaX / 2.0);
            float halveDeltaY = (float)(deltaY / 2.0);

            box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
            box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateRectangle(Environment.GetWorld().FarSeerWorld, Math.Max(1, (float)deltaX), Math.Max(1, (float)deltaY), 1f);
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.LinearDamping = 1f;
            placement.Fixture.Body.AngularDamping = 1f;
            placement.Fixture.Body.Mass = 1000f;

            var obstacle = new Obstacle { Place = placement, Specy = EntityType.Box };
            return obstacle;
        }

        public static IEntity CreateWall(double deltaX, double deltaY)
        {
            double radius = Math.Max(deltaX, deltaY);

            Polygon box = new Polygon();
            float halveDeltaX = (float)(deltaX / 2.0);
            float halveDeltaY = (float)(deltaY / 2.0);

            box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
            box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateRectangle(Environment.GetWorld().FarSeerWorld, Math.Max(1, (float)deltaX), Math.Max(1, (float)deltaY), 1f);
            placement.Fixture.Body.BodyType = BodyType.Static;
            placement.Fixture.Body.LinearDamping = 1f;
            placement.Fixture.Body.AngularDamping = 1f;
            placement.Fixture.Body.Mass = 1000f;

            var obstacle = new Obstacle { Place = placement, Specy = EntityType.Wall };
            return obstacle;
        }

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
            critter.FoodSpecy = EntityType.Avatar;

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 150);
            critter.CharacterSheet.WalkingDistance = 20 * _velocityMultiplier;
            critter.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
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
            critter.FoodSpecy = EntityType.Plant;

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

        internal static Bullet CreateBullet(double damage)
        {
            var bullet = new Bullet();

            float radius = 1;

            Polygon box = new Polygon();
            box.Points.Add(new Vector(0, 0));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, 3, 1);
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.Mass = 1f;
            placement.Fixture.Body.IsBullet = true;
            placement.Fixture.OnCollision += Bullet.OnCollision;
            placement.Fixture.AfterCollision += Bullet.AfterCollision;

            bullet.Placement = placement;
            bullet.Damage = damage;
            bullet.Explodes = false;
            placement.Fixture.UserData = bullet;

            return bullet;
        }

        internal static Bullet CreateRocket(double damage)
        {
            var bullet = new Bullet();

            float radius = 1;

            Polygon box = new Polygon();
            box.Points.Add(new Vector(0, 0));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, 3, 1);
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.Mass = 1f;
            placement.Fixture.Body.IsBullet = true;
            placement.Fixture.OnCollision += Bullet.OnCollision;
            placement.Fixture.AfterCollision += Bullet.AfterCollision;

            bullet.Placement = placement;
            bullet.Damage = damage;
            bullet.Explodes = true;
            placement.Fixture.UserData = bullet;

            return bullet;
        }

        public static ICreature CreateTurret()
        {
            var critter = new Creature(15);
            critter.Brain = new TurretBrain();

            critter.Specy = EntityType.Turret;
            critter.FoodSpecy = EntityType.Avatar;

            //critter.CharacterSheet.MaxAge = Globals.Radomizer.Next(100, 150);
            critter.CharacterSheet.WalkingDistance = 0;
            critter.CharacterSheet.TurningAngle = 1 * _turnMultiplier;
            critter.CharacterSheet.CoolDown = 0.3;
            critter.CharacterSheet.RangeDamage = 1;
            critter.CharacterSheet.MeleeDamage = 0;
            critter.Brain.InitializeSenses();

            return critter;
        }
    }
}
