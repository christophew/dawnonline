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
    class EntityStatus
    {
        private IEntity _entity;
        private bool _isActive;

        public EntityStatus(IEntity entity, bool isActive)
        {
            _entity = entity;
            _isActive = isActive;
        }

        public Hashtable CreatePhotonPacket()
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = _entity.Id;
            dawnEntity[1] = (byte)_entity.Specy;
            dawnEntity[2] = _entity.Place.Position.X;
            dawnEntity[3] = _entity.Place.Position.Y;
            dawnEntity[4] = _entity.Place.Angle;
            dawnEntity[5] = _isActive;

            var creature = _entity as ICreature;
            if (creature != null && creature.SpawnPoint != null)
            {
                dawnEntity[6] = creature.SpawnPoint.Id;
                dawnEntity[7] = (byte)creature.CharacterSheet.Damage.PercentFilled;
                dawnEntity[8] = (byte)creature.CharacterSheet.Fatigue.PercentFilled;
                dawnEntity[9] = (int)creature.CharacterSheet.Score;
            }
            return dawnEntity;
        }

        public bool HasDeltaChanges(EntityStatus previousStatus)
        {
            Debug.Assert(_entity.Id == previousStatus._entity.Id);

            // Backward compatible: only optimize for boxes && walls
            if (_entity.Specy == EntityType.Box ||
                _entity.Specy == EntityType.Wall)
            {
                if (_entity.Place.Position == previousStatus._entity.Place.Position)
                    return false;
            }

            // TODO: better checks
            return true;
        }
    }
}
