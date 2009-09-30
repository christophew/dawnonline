using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Collision
{
    public interface IPolygon
    {
        IList<Vector> Points { get; }
    }
}
