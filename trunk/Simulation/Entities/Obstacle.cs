using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Entities
{
    internal class Obstacle : IEntity
    {
        public EntityType Specy { get; internal set; }
        public Placement Place { get; internal set; }

        internal Obstacle()
        {
            Place = new Placement();
        }
    }
}
