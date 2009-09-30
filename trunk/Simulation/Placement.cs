using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    class Placement : IPlacement
    {
        #region IPlacement Members

        public Placement()
        {
            Position = new Coordinate();
        }

        public IForm Form
        {
            get; set;
        }

        public Coordinate Position
        {
            get; set;
        }

        public double Angle
        {
            get; set;
        }

        public void OffsetPosition(Coordinate position, double angle)
        {
            Position.X += position.X;
            Position.Y += position.Y;
            Angle += angle;

            (Form.Shape as Polygon).Offset((float)position.X, (float)position.Y);
        }


        #endregion
    }
}
