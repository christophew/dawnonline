using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    public class Bullet
    {
        public Placement Placement { get; internal set; }
        public double Damage { get; internal set; }
        public bool Explodes { get; internal set; }
        //public double Force { get; internal set; }

        internal float Range = 50;
        private float MaxForce = 100;

        // State
        public bool Destroyed { get; internal set; }


        public static bool OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var bullet = fixtureA.UserData as Bullet;
            Debug.Assert(bullet != null);

            // Already registered an impact on another target
            if (bullet.Destroyed)
                return false;

            var targetCreature = fixtureB.UserData as Creature;
            if (targetCreature != null)
            {
                targetCreature.TakeBulletDamage(bullet);
            }


            // Experiment: explode
            if (bullet.Explodes)
            {
                var explosion = new Explosion(Environment.GetWorld().FarSeerWorld);
                //explosion.IgnoreWhenInsideShape = true;
                var hits = explosion.Activate(bullet.Placement.Fixture.Body.Position, bullet.Range, bullet.MaxForce);
                foreach (var hit in hits)
                {
                    var explosionTarget = hit.Key.UserData as Creature;
                    if (explosionTarget == null)
                        continue;
                    var distance = MathTools.GetDistance(fixtureA.Body.Position, fixtureB.Body.Position);
                    explosionTarget.TakeExplosionDamage(bullet, distance);
                }
            }

            // return true : acknowledge the collision
            // return false : ignore the collision
            return true;
        }

        public static void AfterCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var bullet = fixtureA.UserData as Bullet;
            Debug.Assert(bullet != null);

            // Bullet already destroyed
            if (bullet.Destroyed)
                return;

            if (!bullet.Explodes)
            {
                // Ricochette when velocity after collision is high enough
                if (fixtureA.Body.LinearVelocity.Length() > 150)
                    return;
            }

            // Destroy on impact
            bullet.Destroyed = true;
            Environment.GetWorld().RemoveBullet(bullet);
        }
    }
}
