using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Entities
{
    public interface IExplosion
    {
        float Size { get; }
        Vector2 Position { get; }
    }
}
