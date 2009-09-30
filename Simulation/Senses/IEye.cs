using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation.Senses
{
    public interface IEye
    {
        IPolygon GetLineOfSight();
    }
}
