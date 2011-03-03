using System;
using System.Collections.Generic;
using DawnOnline.Simulation;

namespace DawnGame
{
    class DawnWorld
    {
        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        private const int MaxX = 3000;
        private const int MaxY = 2000;
        private Creature _avatar = SimulationFactory.CreateAvatar();
        Random _randomize = new Random();

        public Creature Avatar { get { return _avatar; } }
        public DawnOnline.Simulation.Environment Environment { get { return _environment; } }

        public DawnWorld()
        {
            _environment.AddCreature(_avatar,
                                     new Coordinate { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) }, 0);

            BuildWorld();

            //AddCreatures(CreatureType.Rabbit, 300);
            //AddCreatures(CreatureType.Plant, 300);
            AddCreatures(CreatureType.Predator, 100);
        }

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, -20), new Coordinate { X = MaxX / 2.0, Y = -11 }); // Top
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, 20), new Coordinate { X = MaxX / 2.0, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(-20, MaxY), new Coordinate { X = -11, Y = MaxY / 2.0 }); // Left
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(20, MaxY), new Coordinate { X = MaxX + 11, Y = MaxY / 2.0 }); // Right

            // Randow obstacles
            int maxHeight = 200;
            int maxWide = 200;
            for (int i = 0; i < 10; )
            {
                int height = _randomize.Next(maxHeight);
                int wide = _randomize.Next(maxWide);
                var position = new Coordinate(_randomize.Next(MaxX - wide), _randomize.Next(MaxY - height));
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
                                 new Coordinate { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) },
                                 _randomize.Next(6));
            }
        }

        public void ApplyMove(double timeDelta)
        {
            var creatures = new List<Creature>(_environment.GetCreatures());

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.ApplyActionQueue(timeDelta);
            }
        }

        public void MoveAll()
        {
            var creatures = new List<Creature>(_environment.GetCreatures());

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

            var creatures = new List<Creature>(_environment.GetCreatures());

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
