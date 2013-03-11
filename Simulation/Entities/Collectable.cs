﻿using System;
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

            var creature = fixtureB.UserData as Creature;
            //if ((creature != null) && (creature.Specy == EntityType.Avatar))
            // Everybody can take
            if (creature != null)
            {
                collectable.Taken = true;
                creature.Take(collectable);
            }

            return true;
        }
    }
}