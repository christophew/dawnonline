using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DawnOnline.Simulation
{
    class Environment : IEnvironment
    {
        List<ICreature> _creatures = new List<ICreature>();
        List<IPlacement> _obstacles = new List<IPlacement>();

        public void AddCreature(ICreature creature, Coordinate origin, double angle)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.MyEnvironment = this;
            (myCreature.Place as Placement).SetPosition(origin, angle);
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

        public void AddObstacle(IPlacement obstacle, Coordinate origin)
        {
            var myObstacle = obstacle as Placement;

            myObstacle.SetPosition(origin, 0.0);
            _obstacles.Add(obstacle);
        }

        public IList<IPlacement> GetObstacles()
        {
            return _obstacles;
        }

    }
}
