using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public class Placement
    {
        #region IPlacement Members

        internal Placement()
        {
            Position = new Coordinate();
        }

        public Form Form
        {
            get; internal set;
        }

        public Coordinate Position
        {
            get; internal set;
        }

        public double Angle
        {
            get; internal set;
        }

        internal void OffsetPosition(Coordinate position, double angle)
        {
            Position.X += position.X;
            Position.Y += position.Y;
            Angle += angle;

            (Form.Shape as Polygon).Offset((float)position.X, (float)position.Y);
        }


        #endregion
    }
}
