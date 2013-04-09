using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix
{
    class CreatureRepository
    {
        private static CreatureRepository _singleton = new CreatureRepository();
        public static CreatureRepository GetRepository()
        {
            return _singleton;
        }

        private List<ICreature> _repository = new List<ICreature>();

        public void Add(ICreature creature)
        {
            _repository.Add(creature);
        }

        public List<ICreature> GetLivingSpawnpoints()
        {
            var list = new List<ICreature>();

            foreach (var creature in _repository)
            {
                if (creature.Specy != EntityType.SpawnPoint)
                    continue;

                if( creature.Alive)
                    list.Add(creature);
            }

            return list;
        }
    }
}
