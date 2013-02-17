using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnPhotonApp
{
    class EntityPosition : IEntityPhotonPacket
    {
        private IEntity _entity;

        public EntityPosition(IEntity entity)
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

            return dawnEntity;
        }

        public bool HasDeltaChanges(IEntityPhotonPacket previousStatus)
        {
            var myPrevious = previousStatus as EntityPosition;
            Debug.Assert(myPrevious != null);
            Debug.Assert(_entity.Id == myPrevious._entity.Id);

            // Backward compatible: only optimize for boxes && walls
            if (_entity.Specy == EntityType.Box ||
                _entity.Specy == EntityType.Wall)
            {
                if (_entity.Place.Position == myPrevious._entity.Place.Position)
                    return false;
            }

            // TODO: better checks
            return true;
        }
    }
}
