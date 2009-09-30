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

        public void SetPosition(Coordinate position, double angle)
        {
            Position = position;
            Angle = angle;

            (Form.Shape as Polygon).Offset((float)position.X, (float)position.Y);
        }


        #endregion
    }
}
