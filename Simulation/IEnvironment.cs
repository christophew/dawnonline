using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation
{
    public interface IEnvironment
    {
        void AddCreature(ICreature creature, Coordinate origin, double angle);
        void KillCreature(ICreature creature);

        IList<ICreature> GetCreatures();

        void AddObstacle(IPlacement obstacle, Coordinate origin);
        IList<IPlacement> GetObstacles();
    }
}
