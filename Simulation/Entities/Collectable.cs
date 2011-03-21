using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;

namespace DawnOnline.Simulation.Entities
{
    internal class Collectable : Obstacle
    {
        private bool Taken { get; set; }

        public static bool OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var collectable = fixtureA.UserData as Collectable;
            Debug.Assert(collectable != null);

            if (collectable.Taken)
                return false;

           var avatar = fixtureB.UserData as Creature;
            if ((avatar != null) && (avatar.Specy == EntityType.Avatar))
            {
                collectable.Taken = true;
                avatar.Take(collectable);
            }

            return true;
        }
    }
}
