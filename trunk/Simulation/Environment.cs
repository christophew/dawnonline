using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    public class Environment
    {
        public static World FareSeerWorld = new World(Vector2.Zero);

        List<Creature> _creatures = new List<Creature>();
        Dictionary<CreatureType, List<Creature>> _creaturesPerSpecy = new Dictionary<CreatureType, List<Creature>>();
        List<Placement> _obstacles = new List<Placement>();


        //public Environment()
        //{
        //    FareSeerWorld.
        //}

        public void AddCreature(Creature creature, Vector2 origin, double angle)
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
                    _creaturesPerSpecy.Add(myCreature.Specy, new List<Creature>());
                }

                _creaturesPerSpecy[myCreature.Specy].Add(creature);
            }
        }

        private bool IntersectsWithObstacles(Placement place)
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

        public void KillCreature(Creature creature)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            myCreature.Alive = false;
            _creatures.Remove(creature);
            _creaturesPerSpecy[myCreature.Specy].Remove(creature);
        }

        public IList<Creature> GetCreatures()
        {
            return _creatures;
        }

        public IList<Creature> GetCreatures(CreatureType specy)
        {
            if (!_creaturesPerSpecy.ContainsKey(specy))
            {
                _creaturesPerSpecy.Add(specy, new List<Creature>());
            }

            return _creaturesPerSpecy[specy];
        }

        public bool AddObstacle(Placement obstacle, Vector2 origin)
        {
            var myObstacle = obstacle as Placement;

            myObstacle.OffsetPosition(origin, 0.0);

            if (IntersectsWithObstacles(obstacle))
                return false;

            _obstacles.Add(obstacle);

            return true;
        }

        public IList<Placement> GetObstacles()
        {
            return _obstacles;
        }

        public void Update(double timeDelta)
        {
            // Perform actions
            var creatures = new List<Creature>(GetCreatures());
            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.ApplyActionQueue(timeDelta);
                current.ClearActionQueue();
            }

            // Update physics
            FareSeerWorld.Step(MathHelper.Min((float)timeDelta, 1f / 30f));
        }

        internal IList<Creature> GetCreaturesInRange(Vector2 position, double radius)
        {
            return GetCreaturesInRange(position, radius, GetCreatures());
        }

        internal IList<Creature> GetCreaturesInRange(Vector2 position, double radius, CreatureType specy)
        {
            return GetCreaturesInRange(position, radius, GetCreatures(specy));
        }

        private static IList<Creature> GetCreaturesInRange(Vector2 position, double radius, IList<Creature> creatures)
        {
            var list = new List<Creature>();

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
