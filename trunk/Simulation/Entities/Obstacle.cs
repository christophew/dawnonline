using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    internal class Obstacle : IEntity
    {
        private int _id = Globals.GenerateUniqueId();
        public int Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        public EntityTypeEnum EntityType { get; internal set; }
        public Placement Place { get; internal set; }

        internal Obstacle()
        {
            Place = new Placement();
        }
    }
}
