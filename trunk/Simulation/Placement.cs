using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        #endregion
    }
}
