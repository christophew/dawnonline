using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Senses
{
    internal class Bumper
    {
        private Fixture _fixture = null;
        private float _radius = 0.5f;

        internal bool Hit { get; private set; }

        internal Bumper(Creature creature, Vector2 offset)
        {
            _fixture = FixtureFactory.AttachCircle(_radius, 0, creature.Place.Fixture.Body, offset, this);
            _fixture.IsSensor = true;
            _fixture.OnCollision += OnCollision;
        }

        internal void Clear()
        {
            Hit = false;
        }

        public static bool OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var bumper = fixtureA.UserData as Bumper;
            Debug.Assert(bumper != null);

            bumper.Hit = true;
            return false;
        }
    }
}
