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
            Id = entity.Id;
        }

        public int Id { get; private set; }

        public Hashtable CreatePhotonPacket()
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = _entity.Id;
            dawnEntity[1] = _isActive;

            var creature = _entity as ICreature;
            if (creature != null)
            {
                dawnEntity[2] = (byte)creature.CharacterSheet.Damage.PercentFilled;
                dawnEntity[3] = (byte)creature.CharacterSheet.Fatigue.PercentFilled;
                dawnEntity[4] = (int)creature.CharacterSheet.Score;
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

        public DateTime LastUpdateSend { get; set; }
    }
}
