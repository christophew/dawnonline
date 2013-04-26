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
            // Should only be triggered on the server
            Debug.Assert(Globals.GetInstanceId() == 0, "Should only be triggered on the server");

            var collectable = fixtureA.UserData as Collectable;
            Debug.Assert(collectable != null);

            if (collectable.Taken)
                return false;

            var creature = fixtureB.UserData as Creature;
            if (creature != null)
            {
                collectable.Taken = true;
                creature.TryToEat(collectable);
            }

            return true;
        }
    }
}
