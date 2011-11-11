using System;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using System.Linq;

namespace DawnGame
{
    class DawnWorld
    {
        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        private const float MaxX = 300;
        private const float MaxY = 200;
        private IAvatar _avatar = CreatureBuilder.CreateAvatar();
        Random _randomize = new Random();

        private int _grid = 5;

        private int _nrOfSpawnPoints = 5;
        private int _nrOfTreasures = 25;

        public IAvatar Avatar { get { return _avatar; } }
        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            BuildWorld2();

            // Add avatar
            while (!_environment.AddCreature(_avatar,
                                     new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) }, 0))
            {
                 _avatar = CreatureBuilder.CreateAvatar();
            }


            //AddCreatures(EntityType.Rabbit, 300);
            //AddCreatures(EntityType.Plant, 300);
            //AddCreatures(EntityType.Predator, 10);
            //AddCreatures(EntityType.Turret, 30);
            AddSpawnPoints(EntityType.Predator, _nrOfSpawnPoints);
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
            for (int i = 0; i < 300; )
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
            for (int i = 0; i < 150; )
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
            _environment.Update(timeDelta);
        }

        public void MoveAll()
        {
            var creatures = new List<ICreature>(_environment.GetCreatures());

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.Move();

                // Died of old age..
                if (!current.Alive)
                    continue;


                // TEST: grow on organic waste
                //if ((killed != null) && (killed.Specy != CreatureType.Plant))
                //{
                //    if (_randomize.Next(5) == 0)
                //    {
                //        var plant = SimulationFactory.CreatePlant();
                //        _environment.AddCreature(plant, killed.Place.Position, 0);
                //    }
                //}

                // Make sure we always have enough spawnpoints
                var spawnPoints = _environment.GetCreatures(EntityType.SpawnPoint);
                if (_environment.GetCreatures(EntityType.SpawnPoint).Count < _nrOfSpawnPoints)
                {
                    if (spawnPoints.Count == 0)
                        break;

                    // find best spawnpoint
                    var bestspawnPoint = spawnPoints[0];
                    foreach (var spawnPoint in spawnPoints)
                    {
                        if (spawnPoint.CharacterSheet.Score > bestspawnPoint.CharacterSheet.Score)
                            bestspawnPoint = spawnPoint;
                    }

                    // Replicate
                    var newSpawnPoint = bestspawnPoint.Replicate();
                    var position = new Vector2 {X = _randomize.Next((int) MaxX), Y = _randomize.Next((int) MaxY)};
                    _environment.AddCreature(newSpawnPoint, position, 0);
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
            return string.Format("Turret: {0}; Predators:{1}", nrOfTurrets, nrOfPredators);
        }
    }
}
