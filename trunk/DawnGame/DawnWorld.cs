using System;
using System.Collections.Generic;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;

namespace DawnGame
{
    class DawnWorld
    {
        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        private const float MaxX = 300;
        private const float MaxY = 200;
        private IAvatar _avatar = CreatureBuilder.CreateAvatar();
        Random _randomize = new Random();

        private int _nrOfSpawnPoints = 10;

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
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -2), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 2), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateWall(-2, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateWall(2, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

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
            // World boundaries
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -2), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 2), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateWall(-2, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateWall(2, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

            double height = 4.8;
            double wide = 4.8;

            // Factories
            //for (int i = 0; i < 1; )
            //{
            //    if (CreatePredatorFactory())
            //        i++;
            //}

            int grid = 5;
            // walls
            for (int i = 0; i < 100; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / grid) * grid, _randomize.Next((int)MaxY / grid) * grid);

                int maxLength = 5;

                // Horizontal/Vertical switch
                if (_randomize.Next(2) == 0)
                {
                    int maxHorizontal = _randomize.Next(maxLength);
                    for (int j = 0; j < maxHorizontal; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wide, height), position + new Vector2(0, grid * j))) i++;
                    }
                }
                else
                {
                    int maxVertical = _randomize.Next(maxLength);
                    for (int j = 0; j < maxVertical; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wide, height), position + new Vector2(grid * j, 0))) i++;
                    }
                }
            }

            // Boxes
            for (int i = 0; i < 50; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / grid) * grid, _randomize.Next((int)MaxY / grid) * grid);
                var box = ObstacleBuilder.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }

            // Treasure
            for (int i = 0; i < 50; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / grid) * grid, _randomize.Next((int)MaxY / grid) * grid);
                var box = ObstacleBuilder.CreateTreasure();

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

                    // TODO: find best spawnpoint
                    var bestspawnPoint = spawnPoints[0];
                }
            }

            // Repopulate
            //{
            //    if (nrOfPlants == 0) AddCreatures(CreatureType.Plant, 10);
            //    if (nrOfPredators == 0) AddCreatures(CreatureType.Predator, 10);
            //    if (nrOfRabbits == 0) AddCreatures(CreatureType.Rabbit, 10);
            //}
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
