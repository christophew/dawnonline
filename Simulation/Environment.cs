using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DawnOnline.Simulation.Collision;

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
            (myCreature.Place as Placement).OffsetPosition(origin, angle);

            if (!IntersectsWithObstacles(myCreature.Place))
                _creatures.Add(creature);
        }

        private bool IntersectsWithObstacles(IPlacement place)
        {
            foreach (var current in _obstacles)
            {
                Polygon obstaclePolygon = current.Form.Shape as Polygon;
                PolygonCollisionResult collitionResult = CollisionDetection.PolygonCollision(place.Form.Shape as Polygon,
                                                                                             obstaclePolygon,
                                                                                             new Vector());

                if (collitionResult.Intersect)
                {
                    return true;
                }
            }

            return false;
        }

        public void KillCreature(ICreature creature)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.Alive = false;
            _creatures.Remove(creature);
        }

        public IList<ICreature> GetCreatures()
        {
            return _creatures;
        }

        public bool AddObstacle(IPlacement obstacle, Coordinate origin)
        {
            var myObstacle = obstacle as Placement;

            myObstacle.OffsetPosition(origin, 0.0);

            if (IntersectsWithObstacles(obstacle))
                return false;

            _obstacles.Add(obstacle);
            return true;
        }

        public IList<IPlacement> GetObstacles()
        {
            return _obstacles;
        }

    }
}
