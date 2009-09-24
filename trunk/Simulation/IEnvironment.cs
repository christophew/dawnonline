using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
{
    public interface IEnvironment
    {
        void AddCreature(ICreature creature, Coordinate origin, double angle);
        void KillCreature(ICreature creature);

        IList<ICreature> GetCreatures();
    }
}
