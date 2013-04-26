using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using SharedConstants;

namespace DawnOnline.Simulation.Builders
{
    public static class ObstacleBuilder
    {
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
            placement.Fixture = BodyFactory.CreateRectangle(Environment.GetWorld().FarSeerWorld, Math.Max(1, (float)deltaX), Math.Max(1, (float)deltaY), 1f).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.LinearDamping = 0.3f;
            placement.Fixture.Body.AngularDamping = 2f;
            placement.Fixture.Body.Mass = 300f;

            var obstacle = new Obstacle { Place = placement, EntityType = EntityTypeEnum.Box };
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
            placement.Fixture = BodyFactory.CreateRectangle(Environment.GetWorld().FarSeerWorld, Math.Max(1, (float)deltaX), Math.Max(1, (float)deltaY), 1f).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Static;
            placement.Fixture.Body.LinearDamping = 1f;
            placement.Fixture.Body.AngularDamping = 1f;
            placement.Fixture.Body.Mass = 1000f;

            var obstacle = new Obstacle { Place = placement, EntityType = EntityTypeEnum.Wall };
            return obstacle;
        }

        public static IEntity CreateTreasure(CreatureTypeEnum creatureType, bool bindCollision = true)
        {
            var treasure = new Collectable();

            float radius = 0.5f;

            Polygon box = new Polygon();
            float halveDeltaX = (float)(radius);
            float halveDeltaY = (float)(radius);

            box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
            box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = BodyFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.LinearDamping = 0.5f;
            placement.Fixture.Body.AngularDamping = 0.5f;
            placement.Fixture.Body.Mass = 1f;

            if (bindCollision)
                placement.Fixture.OnCollision += Collectable.OnCollision;

            treasure.Place = placement;
            treasure.EntityType = EntityTypeEnum.Treasure;
            treasure.CreatureType = creatureType;
            placement.Fixture.UserData = treasure;

            return treasure;
        }

        public static IEntity CreatePredatorFactory()
        {
            var factory = new Structure();

            float floorRadius = 15;
            float radius = 10;

            // Temp: untill Polygons are also moved to farseer
            Polygon box = new Polygon();
            {
                float halveDeltaX = (float)(floorRadius);
                float halveDeltaY = (float)(floorRadius);

                box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
                box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
                box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
                box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
                box.BuildEdges();
            }

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = BodyFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Static;

            factory.Place = placement;
            factory.EntityType = EntityTypeEnum.PredatorFactory;
            placement.Fixture.UserData = factory;

            return factory;
        }   
    }
}
