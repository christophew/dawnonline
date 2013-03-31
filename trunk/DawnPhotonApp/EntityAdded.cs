using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnPhotonApp
{
    class EntityAdded : IEntityPhotonPacket
    {
        private IEntity _entity;

        public EntityAdded(IEntity entity)
        {
            _entity = entity;
            Id = entity.Id;
        }

        public int Id { get; private set; }

        public Hashtable CreatePhotonPacket()
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = _entity.Id;
            dawnEntity[1] = (byte)_entity.Specy;
            dawnEntity[2] = _entity.Place.Position.X;
            dawnEntity[3] = _entity.Place.Position.Y;
            dawnEntity[4] = _entity.Place.Angle;

            var creature = _entity as ICreature;
            if (creature != null && creature.SpawnPoint != null)
            {
                dawnEntity[5] = creature.SpawnPoint.Id;
            }

            return dawnEntity;
        }

        public bool HasDeltaChanges(IEntityPhotonPacket previousStatus)
        {
            return true;
        }

        public DateTime LastUpdateSend { get; set; }
    }
}
