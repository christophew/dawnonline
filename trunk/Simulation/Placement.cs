using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    public class Placement
    {
        internal Placement()
        {
        }

        public Form Form
        {
            get; internal set;
        }

        public Vector2 Position { get { return Fixture.Body.Position; }}

        public float Angle { get { return Fixture.Body.Rotation; } }

        public float Velocity { get { return Fixture.Body.LinearVelocity.Length(); } }


        internal Fixture Fixture { get; set; }

        internal void OffsetPosition(Vector2 position, double angle)
        {
            (Form.Shape as Polygon).Offset((float)position.X, (float)position.Y);

            Fixture.Body.Position = new Vector2(position.X, position.Y);
            Fixture.Body.Rotation = (float)angle;
        }
    }
}
