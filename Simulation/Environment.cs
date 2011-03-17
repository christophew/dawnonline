using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{

    public class Environment
    {
        private static Environment _theWorld = new Environment();
        internal static Environment GetWorld()
        {
            return _theWorld;
        }


        internal World FarSeerWorld { get; private set; }

        List<ICreature> _creatures = new List<ICreature>();
        Dictionary<EntityType, List<ICreature>> _creaturesPerSpecy = new Dictionary<EntityType, List<ICreature>>();
        List<IEntity> _obstacles = new List<IEntity>();
        List<Bullet> _bullets = new List<Bullet>();


        private Environment()
        {
            FarSeerWorld = new World(Vector2.Zero);
        }

        public void AddCreature(ICreature creature, Vector2 origin, double angle)
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

                _creaturesPerSpecy[myCreature.Specy].Add(myCreature);
            }
        }

        private bool IntersectsWithObstacles(Placement place)
        {
            foreach (var current in _obstacles)
            {
                Polygon obstaclePolygon = current.Place.Form.Shape as Polygon;
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

        internal void KillCreature(Creature creature)
        {
            Debug.Assert(creature != null);

            creature.Alive = false;
            _creatures.Remove(creature);
            _creaturesPerSpecy[creature.Specy].Remove(creature);

            FarSeerWorld.RemoveBody(creature.Place.Fixture.Body);
        }

        public IList<ICreature> GetCreatures()
        {
            return _creatures;
        }

        public IList<ICreature> GetCreatures(EntityType specy)
        {
            if (!_creaturesPerSpecy.ContainsKey(specy))
            {
                _creaturesPerSpecy.Add(specy, new List<ICreature>());
            }

            return _creaturesPerSpecy[specy];
        }

        public bool AddObstacle(IEntity obstacle, Vector2 origin)
        {
            obstacle.Place.OffsetPosition(origin, 0.0);


            // Don't check on intersections => causes ghost boxes
            // TODO: return false should also destroy the fixture in the FarseerWorld
            //if (IntersectsWithObstacles(obstacle))
            //    return false;

            _obstacles.Add(obstacle);

            return true;
        }

        public bool AddBullet(Bullet bullet, Vector2 origin)
        {
            bullet.Placement.OffsetPosition(origin, 0.0);

            //if (IntersectsWithObstacles(obstacle))
            //    return false;

            _bullets.Add(bullet);

            return true;
        }

        public void RemoveBullet(Bullet bullet)
        {
            FarSeerWorld.RemoveBody(bullet.Placement.Fixture.Body);
            _bullets.Remove(bullet);
        }

        public IList<IEntity> GetObstacles()
        {
            return _obstacles;
        }

        public IList<Bullet> GetBullets()
        {
            return _bullets;
        }

        public void Update(double timeDelta)
        {
            // Perform actions
            var creatures = new List<ICreature>(GetCreatures());
            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.ApplyActionQueue(timeDelta);
                current.ClearActionQueue();
            }

            // Update physics
            FarSeerWorld.Step(MathHelper.Min((float)timeDelta, 1f / 30f));
        }

        internal IList<Creature> GetCreaturesInRange(Vector2 position, double radius)
        {
            var creatures = GetCreatures() as IList<Creature>;
            return GetCreaturesInRange(position, radius, creatures);
        }

        internal IList<Creature> GetCreaturesInRange(Vector2 position, double radius, EntityType specy)
        {
            var creatures = GetCreatures(specy) as IList<Creature>;
            return GetCreaturesInRange(position, radius, creatures);
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
