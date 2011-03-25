﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace DawnOnline.Simulation.Builders
{
    internal static class BulletBuilder
    {
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
    }
}
