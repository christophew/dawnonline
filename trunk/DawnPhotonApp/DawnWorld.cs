using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using System.Linq;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnGame
{
    class DawnWorld
    {
        public const float MaxX = WorldConstants.MaxX;
        public const float MaxY = WorldConstants.MaxY;

        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment(0); // 0 = server id
        Random _randomize = new Random();

        private int _grid = 5;

        //private int _nrOfSpawnPoints = 0;
        private int _nrOfTreasures = 0;
        private int _nrOfPlants = 75;
        private int _nrOfWalls = 300;
        private int _maxWallLength = 10;
        private int _nrOfBoxes = 0;
        private int _stablePopulationSize = 500;

        private int _nrOfSpawnPointsReplicated = 0;

        private static int _agingDeltaInSeconds = 5;
        private static int _agingImpact = 1;



        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            BuildWorld2();


            //AddCreatures(EntityTypeEnum.Rabbit, 300);
            //AddCreatures(EntityTypeEnum.Plant, 300);
            //AddCreatures(EntityTypeEnum.Predator, 300);
            //AddCreatures(EntityTypeEnum.Turret, 30);
            //AddSpawnPoints(EntityTypeEnum.Predator, _nrOfSpawnPoints);
        }

        public ICreature AddAvatar()
        {
            ICreature avatar = CreatureBuilder.CreateAvatar();
            while (!_environment.AddCreature(avatar,
                                     new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) }, 0))
            {
                avatar = CreatureBuilder.CreateAvatar();
            }

            return avatar;
        }

        public ICreature GetAvatar(int id)
        {
            var avatar = _environment.GetCreatures(CreatureTypeEnum.Avatar).FirstOrDefault(a => a.Id == id);
            return avatar;
        }

        public ICreature GetCreature(int id)
        {
            var avatar = _environment.GetCreatures().FirstOrDefault(a => a.Id == id);
            return avatar;
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
            // World boundaries
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = -_grid }); // Top
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 2 * _grid), new Vector2 { X = MaxX / 2.0f, Y = MaxY + _grid }); // Bottom
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(-2 * _grid, MaxY), new Vector2 { X = -_grid, Y = MaxY / 2.0f }); // Left
            //_environment.AddObstacle(ObstacleBuilder.CreateWall(2, MaxY * _grid), new Vector2 { X = MaxX + _grid, Y = MaxY / 2.0f }); // Right
            for (int i = 0; i <= MaxX; i += _grid)
            {
                _environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), new Vector2 { X = i, Y = 0 });
                _environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), new Vector2 { X = i, Y = MaxY });
            }
            for (int i = 0; i <= MaxY; i += _grid)
            {
                _environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), new Vector2 { X = 0, Y = i });
                _environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), new Vector2 { X = MaxX, Y = i });
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

                // Horizontal/Vertical switch
                if (_randomize.Next(2) == 0)
                {
                    int maxHorizontal = _randomize.Next(_maxWallLength);
                    for (int j = 0; j < maxHorizontal; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), position + new Vector2(0, _grid * j))) i++;
                    }
                }
                else
                {
                    int maxVertical = _randomize.Next(_maxWallLength);
                    for (int j = 0; j < maxVertical; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(WorldConstants.WallHeight, WorldConstants.WallWide), position + new Vector2(_grid * j, 0))) i++;
                    }
                }
            }

            // Boxes
            for (int i = 0; i < _nrOfBoxes; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / _grid) * _grid, _randomize.Next((int)MaxY / _grid) * _grid);
                var box = ObstacleBuilder.CreateObstacleBox(WorldConstants.WallHeight, WorldConstants.WallWide);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        //private bool CreatePredatorFactory()
        //{
        //    var position = new Vector2(_randomize.Next((int)MaxX / 10) * 10, _randomize.Next((int)MaxY / 10) * 10);
        //    var box = ObstacleBuilder.CreatePredatorFactory();

        //    if (!_environment.AddObstacle(box, position))
        //        return false;

        //    // + SpawnPoints
        //    var offset = 125;
        //    _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityTypeEnum.Predator), new Vector2(position.X + offset, position.Y), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityTypeEnum.Predator), new Vector2(position.X - offset, position.Y), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityTypeEnum.Predator), new Vector2(position.X, position.Y - offset), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateSpawnPoint(EntityTypeEnum.Predator), new Vector2(position.X, position.Y - offset), 0, false);

        //    // + turrets
        //    _environment.AddCreature(CreatureBuilder.CreateTurret(EntityTypeEnum.Avatar), new Vector2(position.X + offset, position.Y + offset), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateTurret(EntityTypeEnum.Avatar), new Vector2(position.X - offset, position.Y + offset), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateTurret(EntityTypeEnum.Avatar), new Vector2(position.X - offset, position.Y - offset), 0, false);
        //    _environment.AddCreature(CreatureBuilder.CreateTurret(EntityTypeEnum.Avatar), new Vector2(position.X + offset, position.Y - offset), 0, false);

        //    return true;
        //}

        public IEntity AddCreature(EntityTypeEnum entityType, CreatureTypeEnum creatureType, Vector2 position, float angle, int spawnPointId, int id)
        {
            // Vector(0,0) = position undefined
            if (position.X == 0 && position.Y == 0)
            {
                position = new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) };
                
            }

            ICreature spawnPoint = null;
            if (spawnPointId != 0)
            {
                spawnPoint = _environment.GetCreatures().FirstOrDefault(c => c.Id == spawnPointId);
            }

            var newCreature = CloneBuilder.CreateCreature(entityType, creatureType, spawnPoint, id);
            if (_environment.AddCreature(newCreature, position, angle, false))
            {
                return newCreature;
            }

            throw new NotSupportedException("TODO");
        }

        public void ApplyMove(double timeDelta)
        {
            _environment.ApplyActions(timeDelta);

            // Make sure we always have enough Treasure
            {
                var obstacles = _environment.GetObstacles();
                if (obstacles.Where(o => o.EntityType == EntityTypeEnum.Treasure).Count() < _nrOfTreasures)
                {
                    var position = new Vector2(_randomize.Next((int)MaxX / _grid) * _grid, _randomize.Next((int)MaxY / _grid) * _grid);
                    var box = ObstacleBuilder.CreateTreasure();
                    _environment.AddObstacle(box, position);
                }
            }

            // Make sure we always have enough Plants
            {
                var plants = _environment.GetCreatures(CreatureTypeEnum.Plant);
                if (plants.Count() < _nrOfPlants)
                {
                    var position = new Vector2(_randomize.Next((int)MaxX), _randomize.Next((int)MaxY));
                    var plant = CreatureBuilder.CreatePlant(new DummyBrain());
                    _environment.AddCreature(plant, position, 0);
                }
            }
        }


        private static DateTime _lastAgingApplied;

        public void UpdatePhysics(double timeDelta)
        {
            _environment.UpdatePhysics(timeDelta);
            _environment.UpdateSounds(timeDelta);


            // Keep population at bay
            if (_environment.GetCreatures().Count > _stablePopulationSize)
            {
                //_environment.Armageddon(_environment.GetCreatures().Count/2);
                //_environment.WrathOfGod(10);
                _environment.Earthquake(20);
            }

            // Age
            if ((DateTime.Now - _lastAgingApplied).TotalSeconds > _agingDeltaInSeconds)
            {
                _environment.ApplyAging(_agingImpact);
                _lastAgingApplied = DateTime.Now;
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

                if (current.CreatureType == CreatureTypeEnum.Plant) nrOfPlants++;
                if (current.CreatureType == CreatureTypeEnum.Rabbit) nrOfRabbits++;
                if (current.CreatureType == CreatureTypeEnum.Predator) nrOfPredators++;
                if (current.CreatureType == CreatureTypeEnum.Turret) nrOfTurrets++;
            }

            //return string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
            return string.Format("Turret: {0}; Predators: {1}; SpawnPointsReplicated: {2}", nrOfTurrets, nrOfPredators, _nrOfSpawnPointsReplicated);
        }
    }
}
