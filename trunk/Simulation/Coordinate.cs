using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public class Coordinate
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Coordinate()
        {
        }

        public Coordinate(Vector vector)
        {
            X = vector.X;
            Y = vector.Y;
        }
    }
}
