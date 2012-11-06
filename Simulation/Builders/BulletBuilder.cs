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
    internal static class BulletBuilder
    {
        internal static Bullet CreateBullet(double damage)
        {
            var bullet = new Bullet();

            float radius = 0.1f;

            Polygon box = new Polygon();
            box.Points.Add(new Vector(0, 0));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = BodyFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.Mass = .1f;
            placement.Fixture.Body.IsBullet = true;
            placement.Fixture.OnCollision += Bullet.OnCollision;
            placement.Fixture.AfterCollision += Bullet.AfterCollision;

            // Bullets can't hit bullets (yet)
            placement.Fixture.CollisionGroup = -1;

            bullet.Place = placement;
            bullet.Damage = damage;
            bullet.Explodes = false;
            bullet.Specy = EntityType.Bullet;
            placement.Fixture.UserData = bullet;

            return bullet;
        }

        internal static Bullet CreateRocket(double damage)
        {
            var bullet = new Bullet();

            float radius = 0.2f;

            Polygon box = new Polygon();
            box.Points.Add(new Vector(0, 0));
            box.BuildEdges();

            var form = new Form { BoundingCircleRadius = radius, Shape = box };
            var placement = new Placement { Form = form };
            placement.Fixture = BodyFactory.CreateCircle(Environment.GetWorld().FarSeerWorld, radius, 1).FixtureList[0];
            placement.Fixture.Body.BodyType = BodyType.Dynamic;
            placement.Fixture.Body.Mass = .15f;
            placement.Fixture.Body.IsBullet = true;
            placement.Fixture.OnCollision += Bullet.OnCollision;
            placement.Fixture.AfterCollision += Bullet.AfterCollision;

            // Bullets can't hit bullets (yet)
            placement.Fixture.CollisionGroup = -1;

            bullet.Place = placement;
            bullet.Damage = damage;
            bullet.Explodes = true;
            bullet.Specy = EntityType.Rocket;
            placement.Fixture.UserData = bullet;

            return bullet;
        }
    }
}
