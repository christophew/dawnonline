using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Entities
{
    internal class Obstacle : IEntity
    {
        private readonly int _id = Globals.GenerateUniqueId();
        public int Id { get { return _id; } }

        public EntityType Specy { get; internal set; }
        public Placement Place { get; internal set; }

        internal Obstacle()
        {
            Place = new Placement();
        }
    }
}
