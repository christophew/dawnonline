using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public class Form
    {
        public double BoundingCircleRadius
        {
            get; internal set;
        }

        public IPolygon Shape
        {
            get; internal set;
        }
    }
}
