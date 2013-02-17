using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnPhotonApp
{
    class EntityLoad : IEntityPhotonPacket
    {
        private IEntity _entity;

        public EntityLoad(IEntity entity)
        {
            _entity = entity;
        }

        public Hashtable CreatePhotonPacket()
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = _entity.Id;
            dawnEntity[1] = _entity.Place.Position.X;
            dawnEntity[2] = _entity.Place.Position.Y;
            dawnEntity[3] = _entity.Place.Angle;
            dawnEntity[4] = (byte)_entity.Specy;

            return dawnEntity;
        }

        public bool HasDeltaChanges(IEntityPhotonPacket previousStatus)
        {
            return true;
        }    
    }
}
