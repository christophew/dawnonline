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
        internal void UpdateEntity(DawnClientEntity entity)
        {
            lock (this)
            {
                _entities[entity.Id] = entity;
            }
        }    
    
        internal void UpdateEntities(List<DawnClientEntity> entities)
        {
            lock (this)
            {
                foreach (var entity in entities)
                {
                    _entities[entity.Id] = entity;
                }
            }
        }    
    
        internal void RemoveEntities(Hashtable ids)
        {
            lock (this)
            {
                foreach (int id in ids.Values)
                {
                    _entities.Remove(id);
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
