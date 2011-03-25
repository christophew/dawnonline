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
        private const float MaxX = 3000;
        private const float MaxY = 2000;
        private IAvatar _avatar = CreatureBuilder.CreateAvatar();
        Random _randomize = new Random();

        public IAvatar Avatar { get { return _avatar; } }
        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            _environment.AddCreature(_avatar,
                                     new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) }, 0);

            BuildWorld2();

            //AddCreatures(EntityType.Rabbit, 300);
            //AddCreatures(EntityType.Plant, 300);
            AddCreatures(EntityType.Predator, 50);
            AddCreatures(EntityType.Turret, 30);
        }

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -20), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 20), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateWall(-20, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateWall(20, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

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
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, -20), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateWall(MaxX, 20), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateWall(-20, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateWall(20, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

            int height = 48;
            int wide = 48;

            // Factories
            for (int i = 0; i < 1; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / 100) * 100, _randomize.Next((int)MaxY / 100) * 100);
                var box = ObstacleBuilder.CreatePredatorFactory();

                if (_environment.AddObstacle(box, position))
                    i++;
            }

            // walls
            for (int i = 0; i < 500; )
            {
                var grid = 50;
                var maxLength = 5;
                var position = new Vector2(_randomize.Next((int)MaxX / grid) * grid, _randomize.Next((int)MaxY / grid) * grid);

                if (_randomize.Next(2) == 0)
                {
                    int maxHorizontal = _randomize.Next(maxLength);
                    for (int j = 0; j < maxHorizontal; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wide, height), position + new Vector2(0, 50 * j))) i++;
                    }
                }
                else
                {
                    int maxVertical = _randomize.Next(maxLength);
                    for (int j = 0; j < maxVertical; j++)
                    {
                        if (_environment.AddObstacle(ObstacleBuilder.CreateWall(wide, height), position + new Vector2(50 * j, 0))) i++;
                    }
                }
            }

            // Boxes
            for (int i = 0; i < 150; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / 50) * 50, _randomize.Next((int)MaxY / 50) * 50);
                var box = ObstacleBuilder.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }

            // Treasure
            for (int i = 0; i < 50; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / 50) * 50, _randomize.Next((int)MaxY / 50) * 50);
                var box = ObstacleBuilder.CreateTreasure();

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private void AddCreatures(EntityType specy, int amount)
        {
            for (int i = 0; i < amount;)
            {
                if (_environment.AddCreature(CreatureBuilder.CreateCreature(specy),
                                 new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) },
                                 _randomize.Next(6)))
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
