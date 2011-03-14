using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;

namespace DawnOnline.Simulation
{
    public class Bullet
    {
        public Placement Placement { get; internal set; }
        public double Damage { get; internal set; }

        // State
        public bool Exploded { get; internal set; }


        public static bool OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var bullet = fixtureA.UserData as Bullet;
            Debug.Assert(bullet != null);

            // Already registered an impact on another target
            if (bullet.Exploded)
                return false;

            var targetCreature = fixtureB.UserData as Creature;
            if (targetCreature != null)
            {
                targetCreature.TakeBulletDamage(bullet);
            }


            // Experiment: explode
            {
                var explosion = new Explosion(Environment.GetWorld().FarSeerWorld);
                //explosion.IgnoreWhenInsideShape = true;
                explosion.Activate(bullet.Placement.Fixture.Body.Position, 50, 200);
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
            if (bullet.Exploded)
                return;

            // Ricochette when velocity after collision is high enough
            if (fixtureA.Body.LinearVelocity.Length() > 100)
                return;

            // Destroy on impact
            bullet.Exploded = true;
            Environment.GetWorld().RemoveBullet(bullet);
        }
    }
}
