using System.Diagnostics;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    internal class Bullet : IEntity
    {
        private readonly int _id = Globals.GenerateUniqueId();
        public int Id { get { return _id; } }

        public Placement Place { get; internal set; }
        public EntityTypeEnum EntityType { get; internal set; }

        public double Damage { get; internal set; }
        public bool Explodes { get; internal set; }
        //public double Force { get; internal set; }

        internal float Range = 6;
        private float MaxForce = 5;
        private static float _ricochetteVelocityThreshold = 15;

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
                // Add explosion effect
                {
                    var explosionEffect = new ExplosionEffect(bullet.Place.Fixture.Body.Position, (float)bullet.Range * 2, 75);
                    Environment.GetWorld().AddExplosion(explosionEffect);
                }

                var explosion = new Explosion(Environment.GetWorld().FarSeerWorld);
                //explosion.IgnoreWhenInsideShape = true;
                var hits = explosion.Activate(bullet.Place.Fixture.Body.Position, bullet.Range, bullet.MaxForce);
                foreach (var hit in hits)
                {
                    // Bullets in the explosion area are destroyed
                    var targetAsBullet = hit.Key.UserData as Bullet;
                    if ((targetAsBullet != null) && (targetAsBullet != bullet))
                    {
                        Environment.GetWorld().RemoveBullet(targetAsBullet);
                    }

                    // Apply damage to creatures
                    var explosionTarget = hit.Key.UserData as Creature;
                    if (explosionTarget == null)
                        continue;
                    var distance = MathTools.GetDistance(fixtureA.Body.Position, fixtureB.Body.Position);
                    explosionTarget.TakeExplosionDamage(bullet, distance);

                    // TODO: Apply damage to structures?
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

            // Possible ricochette
            //if (!bullet.Explodes)
            //{
            //    // Ricochette when velocity after collision is high enough
            //    if (fixtureA.Body.LinearVelocity.Length() > _ricochetteVelocityThreshold)
            //        return;
            //}

            // Destroy on impact
            bullet.Destroyed = true;
            Environment.GetWorld().RemoveBullet(bullet);
        }
    }
}
