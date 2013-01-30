using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnPhotonApp
{
    class WorldSyncState
    {
        class EntitySyncState
        {
            public DateTime LastCommandTimestamp = DateTime.Now;

            public bool IsLinkDead
            {
                get { return (DateTime.Now - LastCommandTimestamp).TotalSeconds > 2; }
            }
        }
        private Dictionary<int, EntitySyncState> _synchStates = new Dictionary<int, EntitySyncState>();

        public void EntityCommandReceived(int entityId)
        {
            EntitySyncState entitySyncState;
            if (_synchStates.TryGetValue(entityId, out entitySyncState))
            {
                entitySyncState.LastCommandTimestamp = DateTime.Now;
            }
            else
            {
                _synchStates.Add(entityId, new EntitySyncState());
            }
        }

        public bool IsActive(int entityId)
        {
            EntitySyncState entitySyncState;
            if (!_synchStates.TryGetValue(entityId, out entitySyncState))
                return false;

            return !entitySyncState.IsLinkDead;
        }
    }
}
