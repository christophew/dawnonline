using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DawnGame
{
    class DawnWorld
    {
        public const float MaxX = 500;
        public const float MaxY = 400;

        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        Random _randomize = new Random();

        private int _grid = 5;

        private int _nrOfSpawnPoints = 25;
        private int _nrOfTreasures = 0;
        private int _nrOfWalls = 1500;
        private int _nrOfBoxes = 0;
        private int _stablePopulationSize = 150;

        private int _nrOfSpawnPointsReplicated = 0;

        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            BuildWorld2();


            //AddCreatures(EntityType.Rabbit, 300);
            //AddCreatures(EntityType.Plant, 300);
            //AddCreatures(EntityType.Predator, 1);
            //AddCreatures(EntityType.Turret, 30);
            AddSpawnPoints(EntityType.Predator, _nrOfSpawnPoints);
        }

        public IAvatar AddAvatar()
        {
            IAvatar avatar = CreatureBuilder.CreateAvatar();
            while (!_environment.AddCreature(avatar,
                                     new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) }, 0))
            {
                avatar = CreatureBuilder.CreateAvatar();
            }

            return avatar;
        }

        public IAvatar GetAvatar(int id)
        {
            var avatar = _environment.GetCreatures(EntityType.Avatar).FirstOrDefault(a => a.Id == id);
            return avatar as IAvatar;
        }

        public Vector2 Center
        {
            get
            {
                return new Vector2(MaxX/2f, MaxY/2f);
            }
        }

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = -_grid }); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = MaxY + _grid }); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateWall(-2 * _grid, MaxY), new Vector2 { X = -_grid, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateWall(2, MaxY * _grid), new Vector2 { X = MaxX + _grid, Y = MaxY / 2.0f }); // Right

            // Randow obstacles
            int maxHeight = 200;
            int maxWide = 200;
            for (int i = 0; i < 600; )
            {
                int height = _randomize.Next(maxHeight);
                int wide = _randomize.Next(maxWide);
                var position = new Vector2(_randomize.Next((int)MaxX - wide), _randomize.Next((int)MaxY - height));
                var box = ObstacleBuilder.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private void BuildWorld2()
        {
            const double wallHeight = 4.8;
            const double wallWide = 4.8;

            // World boundaries
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = -_grid }); // Top
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = MaxY + _grid }); // Bottom
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(-2 * _grid, MaxY), new Vector2 { X = -_grid, Y = MaxY / 2.0f }); // Left
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(2, MaxY * _grid), new Vector2 { X = MaxX + _grid, Y = MaxY / 2.0f }); // Right
            for (int i = 0; i <= MaxX; i += _grid)
            {
                _environment.AddObstacle(ObstacleBuilder.CreateWall(wallHeight, wallWide), new Vector2 { X = i, Y = 0 });
                _environment.AddObstacle(ObstacleBuilder.CreateWall(wallHeight, wallWide), new Vector2 { X = i, Y = MaxY });
            }
            for (int i = 0; i <= MaxY; i += _grid)
            {
                _environment.AddObstacle(ObstacleBuilder.CreateWall(wallHeight, wallWide), new Vector2 { X = 0, Y = i });
                _environment.AddObstacle(ObstacleBuilder.CreateWall(wallHeight, wallWide), new Vector2 { X = MaxX, Y = i });
            }

            // Factories
            //for (int i = 0; i < 1; )
            //{
            //    if (CreatePredatorFactory())
            //        i++;
            //}

            // walls
            for (int i = 0; i < _nrOfWalls; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / _grid) * _grid, _randomize.Next((int)MaxY / _grid) * _grid);

                int maxLength = 5;

                // Horizontal/Vertical switch
                if (_randomize.Next(2) == 0)
                {
                    int maxHorizontal = _randomize.Next(maxLength);
                    for (int j = 0; j < maxHorizontal; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wallWide, wallHeight), position + new Vector2(0, _grid * j))) i++;
                    }
                }
                else
                {
                    int maxVertical = _randomize.Next(maxLength);
                    for (int j = 0; j < maxVertical; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wallWide, wallHeight), position + new Vector2(_grid * j, 0))) i++;
                    }
                }
            }

            // Boxes
            for (int i = 0; i < _nrOfBoxes; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / _grid) * _grid, _randomize.Next((int)MaxY / _grid) * _grid);
                var box = ObstacleBuilder.CreateObstacleBox(wallWide, wallHeight);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private bool CreatePredatorFactory()
        {
            var position = new Vector2(_randomize.Next((int)MaxX / 10) * 10, _randomize.Next((int)MaxY / 10) * 10);
            var box = ObstacleBuilder.CreatePredatorFactory();

            if (!_environment.AddObstacle(box, position))
                return false;

            // + SpawnPoints
            var offset = 125;
            _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityType.Predator), new Vector2(position.X + offset, position.Y), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityType.Predator), new Vector2(position.X - offset, position.Y), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityType.Predator), new Vector2(position.X, position.Y - offset), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityType.Predator), new Vector2(position.X, position.Y - offset), 0, false);

            // + turrets
            _environment.AddCreature(CreatureBuilder.CreateTurret(EntityType.Avatar), new Vector2(position.X + offset, position.Y + offset), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateTurret(EntityType.Avatar), new Vector2(position.X - offset, position.Y + offset), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateTurret(EntityType.Avatar), new Vector2(position.X - offset, position.Y - offset), 0, false);
            _environment.AddCreature(CreatureBuilder.CreateTurret(EntityType.Avatar), new Vector2(position.X + offset, position.Y - offset), 0, false);

            return true;
        }

        public void AddCreatures(EntityType specy, int amount)
        {
            for (int i = 0; i < amount;)
            {
                if (_environment.AddCreature(CreatureBuilder.CreateCreature(specy),
                                 new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) },
                                 _randomize.Next(6)))
                    i++;
            }
        }

        public void AddSpawnPoints(EntityType spawnType, int amount)
        {
            for (int i = 0; i < amount;)
            {
                if (_environment.AddCreature(CreatureBuilder.CreateSpawnPoint(spawnType),
                                 new Vector2(_randomize.Next((int)MaxX), _randomize.Next((int)MaxY)), 
                                 0))
                    i++;
            }
        }

        public void ApplyMove(double timeDelta)
        {
            _environment.ApplyActions(timeDelta);

            // Make sure we always have enough spawnpoints
            var spawnPoints = _environment.GetCreatures(EntityType.SpawnPoint);
            if (_environment.GetCreatures(EntityType.SpawnPoint).Count < _nrOfSpawnPoints)
            {
                var timer = new Stopwatch();
                timer.Start();
                ICreature bestspawnPoint, crossoverMate;

                if (spawnPoints.Count == 0)
                {
                    bestspawnPoint = CreatureBuilder.CreateSpawnPoint(EntityType.Predator);
                    crossoverMate = bestspawnPoint;
                }
                else
                {
                    // find best spawnpoint
                    bestspawnPoint = GetBestspawnPoint(spawnPoints);
                    crossoverMate = GetBestspawnPoint(spawnPoints);
                }

                // Replicate
                //AddSpawnPoints(EntityType.Predator, 1);
                var newSpawnPoint = bestspawnPoint.Replicate(crossoverMate);
                var position = new Vector2 {X = _randomize.Next((int) MaxX), Y = _randomize.Next((int) MaxY)};
                _environment.AddCreature(newSpawnPoint, position, 0);
                _nrOfSpawnPointsReplicated++;

                timer.Stop();
                Console.WriteLine("Replicate.timer: " + timer.ElapsedMilliseconds);
            }

            // Make sure we always have enough Treasure
            var obstacles = _environment.GetObstacles();
            if (obstacles.Where(o => o.Specy == EntityType.Treasure).Count() < _nrOfTreasures)
            {
                var position = new Vector2(_randomize.Next((int)MaxX / _grid) * _grid, _randomize.Next((int)MaxY / _grid) * _grid);
                var box = ObstacleBuilder.CreateTreasure();
                _environment.AddObstacle(box, position);
            }
        }

        public void ThinkAll(double maxThinkTime, TimeSpan timeDelta)
        {
            Console.WriteLine(GetWorldInformation());

            // Keep population at bay
            int moved = _environment.Think(maxThinkTime, timeDelta);
            if (moved < _environment.GetCreatures().Count / 2 && _environment.GetCreatures().Count > _stablePopulationSize)
            {
                //_environment.Armageddon(_environment.GetCreatures().Count/2);
                //_environment.WrathOfGod(10);
                _environment.Earthquake(20);
            }
        }

        public void UpdatePhysics(double timeDelta)
        {
            _environment.UpdatePhysics(timeDelta);
            _environment.UpdateSounds(timeDelta);
        }

        private static ICreature GetBestspawnPoint(IList<ICreature> spawnPoints)
        {
            // Absolute

            //ICreature bestspawnPoint = spawnPoints[0];
            //foreach (var spawnPoint in spawnPoints)
            //{
            //    if (spawnPoint.CharacterSheet.Score > bestspawnPoint.CharacterSheet.Score)
            //    {
            //        bestspawnPoint = spawnPoint;
            //    }
            //}
            //return bestspawnPoint;

            // By better chance
            var randomizer = new Random((int)DateTime.Now.Ticks);
            var tempSpawnPoints = spawnPoints.OrderByDescending(sp => sp.CharacterSheet.Score);
            for (; ; )
            {
                foreach (var sp in tempSpawnPoints)
                {
                    if (randomizer.Next(3) == 0)
                    {
                        Console.WriteLine("Score to replicate: " + sp.CharacterSheet.Score);
                        return sp;
                    }
                }
            }
        }

        public string GetWorldInformation()
        {
            int nrOfPlants = 0;
            int nrOfRabbits = 0;
            int nrOfPredators = 0;
            int nrOfTurrets = 0;

            var creatures = new List<ICreature>(_environment.GetCreatures());

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                if (current.Specy == EntityType.Plant) nrOfPlants++;
                if (current.Specy == EntityType.Rabbit) nrOfRabbits++;
                if (current.Specy == EntityType.Predator) nrOfPredators++;
                if (current.Specy == EntityType.Turret) nrOfTurrets++;
            }

            //return string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
            return string.Format("Turret: {0}; Predators: {1}; SpawnPointsReplicated: {2}", nrOfTurrets, nrOfPredators, _nrOfSpawnPointsReplicated);
        }
    }
}
