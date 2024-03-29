﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SharedConstants;
using PerformanceMonitoring;

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
        Dictionary<CreatureTypeEnum, List<ICreature>> _creaturesPerSpecy = new Dictionary<CreatureTypeEnum, List<ICreature>>();
        List<IEntity> _obstacles = new List<IEntity>();
        List<IEntity> _bullets = new List<IEntity>();
        List<IExplosion> _explosions = new List<IExplosion>();
        Dictionary<Sound.SoundTypeEnum, List<Sound>> _soundsPerType = new Dictionary<Sound.SoundTypeEnum, List<Sound>>();

        public int ResourcesInGround { get; internal set; }


        private Environment()
        {
            FarSeerWorld = new World(Vector2.Zero);
        }

        public void AddResourcesInGround(int amount)
        {
            ResourcesInGround += amount;
        }

        public bool AddCreature(ICreature creature, Vector2 origin, double angle, bool checkIntersect = true)
        {
            var myCreature = creature as Creature;
            Debug.Assert(myCreature != null);

            // Add to FarSeer
            myCreature.AddToPhysicsEngine();

            myCreature.MyEnvironment = this;
            (myCreature.Place as Placement).OffsetPosition(origin, angle);

            // TODO: use farseer for obstacle collision
            if (checkIntersect && IntersectsWithObstacles(myCreature.Place))
            {
                FarSeerWorld.RemoveBody(creature.Place.Fixture.Body);
                return false;
            }

            _creatures.Add(creature);

            if (!_creaturesPerSpecy.ContainsKey(myCreature.CreatureType))
            {
                _creaturesPerSpecy.Add(myCreature.CreatureType, new List<ICreature>());
            }

            _creaturesPerSpecy[myCreature.CreatureType].Add(myCreature);

            return true;
        }

        internal void AddExplosion(IExplosion explosion)
        {
            _explosions.Add(explosion);
        }

        public IList<IExplosion> GetExplosions()
        {
            return _explosions;
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

            // Add explosion
            {
                var explosion = new ExplosionEffect(creature.Place.Position, (float)creature.Place.Form.BoundingCircleRadius*10, 75);
                AddExplosion(explosion);
            }

            RemoveCreature(creature);
        }

        private void RemoveCreature(Creature creature)
        {
            creature.Alive = false;
            _creatures.Remove(creature);
            _creaturesPerSpecy[creature.CreatureType].Remove(creature);

            FarSeerWorld.RemoveBody(creature.Place.Fixture.Body);
        }

        public bool RemoveCreature(int id)
        {
            var creature = _creatures.Find(c => c.Id == id) as Creature;
            if (creature == null)
                return false;

            RemoveCreature(creature);
            return true;
        }

        public IList<ICreature> GetCreatures()
        {
            return _creatures;
        }

        public IList<ICreature> GetCreatures(List<CreatureTypeEnum> species)
        {
            var result = new List<ICreature>();
            foreach (var specy in species)
            {
                result.AddRange(GetCreatures(specy));
            }
            return result;
        }

        public IList<ICreature> GetCreatures(CreatureTypeEnum specy)
        {
            if (!_creaturesPerSpecy.ContainsKey(specy))
            {
                _creaturesPerSpecy.Add(specy, new List<ICreature>());
            }

            return _creaturesPerSpecy[specy];
        }

        public bool AddObstacle(IEntity obstacle, Vector2 origin, double angle = 0.0, bool checkIntersect = true)
        {
            obstacle.Place.OffsetPosition(origin, angle);


            // TODO: Collisioncheck using Farseer
            if (checkIntersect && IntersectsWithObstacles(obstacle.Place))
            {
                FarSeerWorld.RemoveBody(obstacle.Place.Fixture.Body);
                return false;
            }

            _obstacles.Add(obstacle);

            return true;
        }

        internal void RemoveObstacle(IEntity obstacle)
        {
            FarSeerWorld.RemoveBody(obstacle.Place.Fixture.Body);
            _obstacles.Remove(obstacle);
        }

        internal bool AddBullet(Bullet bullet, Vector2 origin, double angle)
        {
            bullet.Place.OffsetPosition(origin, angle);

            //if (IntersectsWithObstacles(obstacle))
            //    return false;

            _bullets.Add(bullet);

            return true;
        }

        internal void RemoveBullet(Bullet bullet)
        {
            FarSeerWorld.RemoveBody(bullet.Place.Fixture.Body);
            _bullets.Remove(bullet);
        }

        public IList<IEntity> GetObstacles()
        {
            return _obstacles;
        }

        public IList<IEntity> GetBullets()
        {
            return _bullets;
        }

        public void ApplyActions(double timeDelta)
        {
            // Perform actions
            var creatures = new List<ICreature>(GetCreatures());
            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.Update(timeDelta);
            }

            // Update obstacles
            var obstacles = new List<IEntity>(GetObstacles());
            foreach (var current in obstacles)
            {
                current.Update(timeDelta);
            }



            // Update explosions
            var remainingExplosions = new List<IExplosion>();
            foreach (var explosion in _explosions.Cast<ExplosionEffect>())
            {
                if (!explosion.IsExpired())
                    remainingExplosions.Add(explosion);
            }
            _explosions = remainingExplosions;
        }

        public void UpdateSounds(double timeDelta)
        {
            var newSoundsPerType = new Dictionary<Sound.SoundTypeEnum, List<Sound>>();

            foreach (var soundList in _soundsPerType)
            {
                var newList = soundList.Value.Where(sound => sound.Update((long) timeDelta)).ToList();

                newSoundsPerType.Add(soundList.Key, newList);
            }

            _soundsPerType = newSoundsPerType;
        }

        public void UpdatePhysics(double timeDelta)
        {
            // Update physics
            FarSeerWorld.Step((float)timeDelta / 1000);
            //FarSeerWorld.Step(1f / 30f);
            //FarSeerWorld.Step(MathHelper.Min((float)timeDelta, 1f / 30f));
            //FarSeerWorld.Step((float)timeDelta / 1000);
        }

        public int Think(double maxThinkTime, TimeSpan timeDelta, IList<int> creatureIds = null)
        {
            var startTime = DateTime.Now;
            var maxTime = startTime + new TimeSpan(0, 0, 0, 0, (int)maxThinkTime);

            IEnumerable<IEntity> creatures;

            if (creatureIds == null)
            {
                creatures = GetCreatures()
                                .OrderBy(c => (c as Creature).LatestThinkTime);
            }
            else
            {
                creatures = GetCreatures()
                                .Where(c => creatureIds.Contains(c.Id))
                                .OrderBy(c => (c as Creature).LatestThinkTime);
            }

            int counter = 0;
            foreach (Creature current in creatures)
            {
                if (!current.Alive)
                    continue;

                counter++;
                current.Think(timeDelta);

                var currentNow = DateTime.Now;
                current.LatestThinkTime = currentNow;

                if (currentNow > maxTime)
                    break;
            }

            var time = (DateTime.Now - startTime).TotalMilliseconds;
            Monitoring.Register_Think(Globals.GetInstanceId(), (int)time, counter);
            Console.WriteLine(string.Format("#Think: {0} - Time: {1}", counter, time));

            return counter;
        }

        internal IList<ICreature> GetCreaturesInRange(Vector2 position, double radius)
        {
            return GetCreaturesInRange(position, radius, GetCreatures());
        }

        internal IList<ICreature> GetCreaturesInRange(Vector2 position, double radius, List<CreatureTypeEnum> species)
        {
            return GetCreaturesInRange(position, radius, GetCreatures(species));
        }

        private static IList<ICreature> GetCreaturesInRange(Vector2 position, double radius, IList<ICreature> creatures)
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

        internal void AddSound(Sound sound)
        {
            List<Sound> categoryList;
            if (!_soundsPerType.TryGetValue(sound.SoundType, out categoryList))
            {
                categoryList = new List<Sound>();
                _soundsPerType.Add(sound.SoundType, categoryList);
            }

            categoryList.Add(sound);
        }

        internal List<Sound> GetSounds(Sound.SoundTypeEnum soundType)
        {
            List<Sound> categoryList;
            if (!_soundsPerType.TryGetValue(soundType, out categoryList))
            {
                categoryList = new List<Sound>();
                _soundsPerType.Add(soundType, categoryList);
            }

            return categoryList;
        }

        public void ApplyAging(int ageImpact)
        {
            // TODO: this is a bit too simple

            foreach (var creature in GetCreatures())
            {
                if (creature.CreatureType != CreatureTypeEnum.Avatar)
                {
                    creature.CharacterSheet.Damage.Increase(ageImpact);
                }
            }
        }

        public int GatherResources(int maxRourcesToGather)
        {
            // TODO: some real logic?
            var extractedRources = Globals.Radomizer.Next(maxRourcesToGather + 1);

            if (extractedRources > ResourcesInGround)
            {
                var old = ResourcesInGround;
                ResourcesInGround = 0;
                return old;
            }

            ResourcesInGround -= extractedRources;
            return extractedRources;
        }

        public void WrathOfGod(int nrKilled)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine("WrathOfGod: " + nrKilled);

            for (int i=0; i < nrKilled; i++)
            {
                if (GetCreatures().Count == 0)
                    break;

                var creature = GetCreatures()[Globals.Radomizer.Next(GetCreatures().Count)] as Creature;
                if (creature.CreatureType == CreatureTypeEnum.Avatar)
                    continue;

                KillCreature(creature);
            }
        }

        public void Earthquake(int nrDamaged)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine("Earthquake: " + nrDamaged);

            for (int i = 0; i < nrDamaged; i++)
            {
                if (GetCreatures().Count == 0)
                    break;

                var creature = GetCreatures()[Globals.Radomizer.Next(GetCreatures().Count)] as Creature;
                if (creature.CreatureType == CreatureTypeEnum.Avatar)
                    continue;

                creature.CharacterSheet.Damage.Increase(33);
            }
        }

        public void Armageddon(int survivors)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine("ARMAGEDDON: " + survivors);

            for (;;)
            {
                if (GetCreatures().Count <= survivors)
                    break;

                var creature = GetCreatures()[Globals.Radomizer.Next(GetCreatures().Count)] as Creature;
                if (creature.CreatureType == CreatureTypeEnum.Avatar)
                    continue;

                KillCreature(creature);
            }
        }
    }
}
