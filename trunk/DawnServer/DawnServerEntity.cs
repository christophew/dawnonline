using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;

namespace DawnServer
{
    [Serializable]
    public class DawnServerEntity
    {
        //public EntityType Specy { get; set; }
        //public Vector2 Position { get; set; }
        public float Angle { get; set; }

        /// <summary>
        /// Serialization c'tor
        /// </summary>
        public DawnServerEntity()
        {}

        internal DawnServerEntity(IEntity entity)
        {
            //Specy = entity.Specy;
            //Position = entity.Place.Position;
            Angle = entity.Place.Angle;
        }
    }
}
