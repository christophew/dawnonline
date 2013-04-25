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
        bool SeesACreature(List<CreatureTypeEnum> species, IEntity spawnPointToExclude);
        bool SeesACreature(List<CreatureTypeEnum> species);
        bool SeesACreature(CreatureTypeEnum specy);

        bool SeesAnObstacle(EntityTypeEnum entityType);
    }
}
