using System;
using System.Collections.Generic;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;

namespace DawnGame
{
    class DawnWorld
    {
        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        private const float MaxX = 3000;
        private const float MaxY = 2000;
        private Creature _avatar = SimulationFactory.CreateAvatar();
        Random _randomize = new Random();

        public Creature Avatar { get { return _avatar; } }
        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            _environment.AddCreature(_avatar,
                                     new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) }, 0);

            BuildWorld2();

            //AddCreatures(CreatureType.Rabbit, 300);
            //AddCreatures(CreatureType.Plant, 300);
            //AddCreatures(CreatureType.Predator, 100);
            AddCreatures(CreatureType.Turret, 50);
        }

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, -20), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, 20), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(-20, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(20, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

            // Randow obstacles
            int maxHeight = 200;
            int maxWide = 200;
            for (int i = 0; i < 600; )
            {
                int height = _randomize.Next(maxHeight);
                int wide = _randomize.Next(maxWide);
                var position = new Vector2(_randomize.Next((int)MaxX - wide), _randomize.Next((int)MaxY - height));
                var box = SimulationFactory.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private void BuildWorld2()
        {
            // World boundaries
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, -20), new Vector2 { X = MaxX / 2.0f, Y = -11 }); // Top
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, 20), new Vector2 { X = MaxX / 2.0f, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(-20, MaxY), new Vector2 { X = -11, Y = MaxY / 2.0f }); // Left
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(20, MaxY), new Vector2 { X = MaxX + 11, Y = MaxY / 2.0f }); // Right

            // Rocks
            int height = 48;
            int wide = 48;
            for (int i = 0; i < 600; )
            {
                var position = new Vector2(_randomize.Next((int)MaxX / 50) * 50, _randomize.Next((int)MaxY / 50) * 50);
                var box = SimulationFactory.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private void AddCreatures(CreatureType specy, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _environment.AddCreature(SimulationFactory.CreateCreature(specy),
                                 new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) },
                                 _randomize.Next(6));
            }
        }

        public void ApplyMove(double timeDelta)
        {
            _environment.Update(timeDelta);
        }

        public void MoveAll()
        {
            var creatures = new List<IEntity>(_environment.GetCreatures());

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

            var creatures = new List<IEntity>(_environment.GetCreatures());

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                if (current.Specy == CreatureType.Plant) nrOfPlants++;
                if (current.Specy == CreatureType.Rabbit) nrOfRabbits++;
                if (current.Specy == CreatureType.Predator) nrOfPredators++;
            }

            return string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
        }
    }
}
