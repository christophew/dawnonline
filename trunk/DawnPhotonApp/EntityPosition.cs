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
        private int _id;
        private float _x, _y, _angle;

        public EntityPosition(IEntity entity)
        {
            _id = entity.Id;
            _x = entity.Place.Position.X;
            _y = entity.Place.Position.Y;
            _angle = entity.Place.Angle;
        }

        public int Id
        {
            get { return _id; }
        }

        public Hashtable CreatePhotonPacket()
        {
            var dawnEntity = new Hashtable();

            dawnEntity[0] = _id;
            dawnEntity[1] = _x;
            dawnEntity[2] = _y;
            dawnEntity[3] = _angle;

            return dawnEntity;
        }

        public bool HasDeltaChanges(IEntityPhotonPacket previousStatus)
        {
            var myPrevious = previousStatus as EntityPosition;
            Debug.Assert(myPrevious != null);
            Debug.Assert(_id == myPrevious._id);

            // Backward compatible: only optimize for boxes && walls
            //if (_entity.Specy == EntityType.Box ||
            //    _entity.Specy == EntityType.Wall)
            {
                if (_x == myPrevious._x && _y == myPrevious._y &&
                    _angle == myPrevious._angle)
                    return false;
            }

            // TODO: better checks
            return true;
        }

        public DateTime LastUpdateSend { get; set; }
    }
}
