﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    class Form : IForm
    {
        public double Radius
        {
            get; set;
        }

        public IPolygon Shape
        {
            get; set;
        }
    }
}