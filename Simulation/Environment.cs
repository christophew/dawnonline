using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation
{
    class Environment : IEnvironment
    {
        List<ICreature> _creatures = new List<ICreature>();
        Dictionary<CreatureType, List<ICreature>> _creaturesPerSpecy = new Dictionary<CreatureType, List<ICreature>>();
        List<IPlacement> _obstacles = new List<IPlacement>();

        public void AddCreature(ICreature creature, Coordinate origin, double angle)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.MyEnvironment = this;
            (myCreature.Place as Placement).OffsetPosition(origin, angle);

            if (!IntersectsWithObstacles(myCreature.Place))
            {
                _creatures.Add(creature);

                if (!_creaturesPerSpecy.ContainsKey(myCreature.Specy))
                {
                    _creaturesPerSpecy.Add(myCreature.Specy, new List<ICreature>());
                }

                _creaturesPerSpecy[myCreature.Specy].Add(creature);
            }
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
            _creaturesPerSpecy[myCreature.Specy].Remove(creature);
        }

        public IList<ICreature> GetCreatures()
        {
            return _creatures;
        }

        public IList<ICreature> GetCreatures(CreatureType specy)
        {
            if (!_creaturesPerSpecy.ContainsKey(specy))
            {
                _creaturesPerSpecy.Add(specy, new List<ICreature>());
            }

            return _creaturesPerSpecy[specy];
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

        internal IList<ICreature> GetCreaturesInRange(Coordinate position, double radius)
        {
            return GetCreaturesInRange(position, radius, GetCreatures());
        }

        internal IList<ICreature> GetCreaturesInRange(Coordinate position, double radius, CreatureType specy)
        {
            return GetCreaturesInRange(position, radius, GetCreatures(specy));
        }

        private static IList<ICreature> GetCreaturesInRange(Coordinate position, double radius, IList<ICreature> creatures)
        {
            var list = new List<ICreature>();

            double radius2 = radius * radius;
            foreach (var current in creatures)
            {
                double distance2 = (MathTools.GetDistance2(position, current.Place.Position));
                if (distance2 < radius2)
                    list.Add(current);
            }

            return list;
        }

    }
}
