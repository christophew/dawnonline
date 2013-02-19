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
    class EntityStatus : IEntityPhotonPacket
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
            dawnEntity[2] = _isActive;

            var creature = _entity as ICreature;
            if (creature != null && creature.SpawnPoint != null)
            {
                dawnEntity[3] = creature.SpawnPoint.Id;
                dawnEntity[4] = (byte)creature.CharacterSheet.Damage.PercentFilled;
                dawnEntity[5] = (byte)creature.CharacterSheet.Fatigue.PercentFilled;
                dawnEntity[6] = (int)creature.CharacterSheet.Score;
            }
            return dawnEntity;
        }

        public bool HasDeltaChanges(IEntityPhotonPacket previousStatus)
        {
            var myPrevious = previousStatus as EntityStatus;

            Debug.Assert(myPrevious != null);
            Debug.Assert(_entity.Id == myPrevious._entity.Id);

            // Small opt: remove walls & boxes => there status can't change
            if (_entity.Specy == EntityType.Wall || _entity.Specy == EntityType.Box)
                return false;

            // TODO: better checks
            return true;
        }   
    }
}
