using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Simulation
{
    class Environment : IEnvironment
    {
        List<ICreature> _creatures = new List<ICreature>();

        public void AddCreature(ICreature creature, Coordinate origin, double angle)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.MyEnvironment = this;
            myCreature.SetPosition(origin, angle);
            _creatures.Add(creature);
        }

        public void KillCreature(ICreature creature)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.Alive = false;
            _creatures.Remove(creature);
        }

        //public IList<IPlacement> GetPlacements()
        //{
        //    var placements = new List<IPlacement>();

        //    foreach (Creature current in _creatures)
        //    {
        //        placements.Add(current.Place);
        //    }

        //    return placements;
        //}

        public IList<ICreature> GetCreatures()
        {
            return _creatures;
        }

    }
}
