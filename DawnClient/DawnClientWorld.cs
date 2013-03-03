using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DawnClient
{
    public class DawnClientWorld
    {
        public string WorldInformation { get; internal set; }

        private Dictionary<int, DawnClientEntity> _entities = new Dictionary<int, DawnClientEntity>();
        private HashSet<int> _removed = new HashSet<int>();


        internal void UpdateEntity(DawnClientEntity entity)
        {
            lock (this)
            {
                _entities[entity.Id] = entity;
            }
        }    
    
        internal void UpdateEntities(List<DawnClientEntity> entities, bool canCreateNew)
        {
            lock (this)
            {
                foreach (var entity in entities)
                {
                    // Check already removed (possibly caused by latency on different channels)
                    if (_removed.Contains(entity.Id))
                        continue;

                    DawnClientEntity existingEntity;
                    if (_entities.TryGetValue(entity.Id, out existingEntity))
                    {
                        // Update existing
                        existingEntity.UpdateFrom(entity);
                    }
                    else
                    {
                        // Create new
                        if (!canCreateNew)
                            continue;

                        _entities.Add(entity.Id, entity);
                    }
                }
            }
        }    
    
        internal void RemoveEntities(int[] ids)
        {
            lock (this)
            {
                foreach (int id in ids)
                {
                    _entities.Remove(id);
                    _removed.Add(id);
                }
            }
        }

        public ReadOnlyCollection<DawnClientEntity> GetEntities()
        {
            lock (this)
            {
                var entities = _entities.Select(e => e.Value).ToList();
                var result = new ReadOnlyCollection<DawnClientEntity>(entities);
                return result;
            }
        }
    }
}
