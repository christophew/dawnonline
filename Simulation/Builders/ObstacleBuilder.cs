﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

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

        public static IEntity CreateTreasure()
        {
            var treasure = new Collectable();

            float radius = 5;

            Polygon box = new Polygon();
            float halveDeltaX = (float)(radius / 2.0);
            float halveDeltaY = (float)(radius / 2.0);

            box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
            box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
            box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1);
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.LinearDamping = 0.5f;
            placement.Fixture.Body.AngularDamping = 0.5f;
            placement.Fixture.Body.Mass = 10f;
            placement.Fixture.OnCollision += Collectable.OnCollision;

            treasure.Place = placement;
            treasure.Specy = EntityType.Treasure;
            placement.Fixture.UserData = treasure;

            return treasure;
        }

        public static IEntity CreatePredatorFactory()
        {
            var factory = new Structure();

            float radius = 100;

            // Temp: untill Polygons are also moved to farseer
            Polygon box = new Polygon();
            {
                float halveDeltaX = (float) (radius/2.0);
                float halveDeltaY = (float) (radius/2.0);

                box.Points.Add(new Vector(-halveDeltaX, -halveDeltaY));
                box.Points.Add(new Vector(halveDeltaX, -halveDeltaY));
                box.Points.Add(new Vector(halveDeltaX, halveDeltaY));
                box.Points.Add(new Vector(-halveDeltaX, halveDeltaY));
                box.BuildEdges();
            }

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = FixtureFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1);
            placement.Fixture.Body.BodyType = BodyType.Static;

            factory.Place = placement;
            factory.Specy = EntityType.PredatorFactory;
            placement.Fixture.UserData = factory;

            return factory;
        }
    }
}
