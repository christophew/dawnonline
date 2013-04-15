using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.Simulation.Senses
{
    public interface IEye
    {
        double DistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true);
        double WeightedDistanceToFirstVisible(List<IEntity> sortedEntities, bool useLineOfSight = true);

        bool SeesCreature(ICreature creature);
        bool SeesACreature(List<EntityType> species, IEntity spawnPointToExclude);
        bool SeesACreature(List<EntityType> species);
        bool SeesACreature(EntityType specy);

        bool SeesAnObstacle(EntityType entityType);
    }
}
