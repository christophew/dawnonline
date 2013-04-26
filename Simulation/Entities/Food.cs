using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Builders;
using FarseerPhysics.Dynamics;

namespace DawnOnline.Simulation.Entities
{
    internal class Food : Obstacle
    {
        private DateTime _creationTime = DateTime.Now;
        private double _timeToLive = 5000;

        private bool Taken { get; set; }

        public override void Update(double timeDelta)
        {
            if ((DateTime.Now - _creationTime).TotalMilliseconds < _timeToLive)
                return;

            var position = this.Place.Position;


            var environment = Environment.GetWorld();

            environment.RemoveObstacle(this);

            // Grow plant
            var plant = CreatureBuilder.CreatePlant();
            environment.AddCreature(plant, position, 0);
        }

        public static bool OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // Should only be triggered on the server
            Debug.Assert(Globals.GetInstanceId() == 0, "Should only be triggered on the server");

            var collectable = fixtureA.UserData as Food;
            Debug.Assert(collectable != null);

            if (collectable.Taken)
                return false;

            var creature = fixtureB.UserData as Creature;
            if (creature != null)
            {
                collectable.Taken = creature.TryToEat(collectable);
            }

            return true;
        }
    }
}
