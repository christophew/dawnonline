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
            var creature = _entity as ICreature;

            var dawnEntity = new Hashtable();
            dawnEntity[0] = _entity.Id;
            dawnEntity[1] = (byte)_entity.EntityType;
            dawnEntity[2] = (byte)(creature != null ? creature.CreatureType : 0);
            dawnEntity[3] = _entity.Place.Position.X;
            dawnEntity[4] = _entity.Place.Position.Y;
            dawnEntity[5] = _entity.Place.Angle;

            if (creature != null && creature.SpawnPoint != null)
            {
                dawnEntity[6] = creature.SpawnPoint.Id;
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
