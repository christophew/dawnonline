﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
{
    public interface IPlacement
    {
        IForm Form { get; }
        Coordinate Position { get; }
        double Angle { get; }
    }
}
