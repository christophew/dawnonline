using System;
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
    
        internal void RemoveEntity(int id)
        {
            lock (this)
            {
                _entities.Remove(id);
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
