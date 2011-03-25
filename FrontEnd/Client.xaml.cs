using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;

namespace FrontEnd
{
    public partial class Window1 : Window
    {
        private readonly DawnOnline.Simulation.Environment _environment = SimulationFactory.CreateEnvironment();
        private const int MaxX = 3000;
        private const int MaxY = 2000;
        private IAvatar _avatar = CreatureBuilder.CreateAvatar();
        private DateTime _lastMove = DateTime.Now;

        Random _randomize = new Random();

        public Window1()
        {
            InitializeComponent();
        }

        private void AddRabbits_Click(object sender, RoutedEventArgs e)
        {
            AddCreatures(EntityType.Rabbit, 30);
        }

        private void AddPlants_Click(object sender, RoutedEventArgs e)
        {
            AddCreatures(EntityType.Plant, 30);
        }

        private void AddPredators_Click(object sender, RoutedEventArgs e)
        {
            AddCreatures(EntityType.Predator, 30);
        }

        private void AddCreatures(EntityType specy, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _environment.AddCreature(CreatureBuilder.CreateCreature(specy),
                                         new Vector2(_randomize.Next(MaxX), _randomize.Next(MaxY)),
                                         _randomize.Next(6));
            }
        }

        private void MoveAll()
        {
            var creatures = new List<ICreature>(_environment.GetCreatures());

            int nrOfPlants = 0;
            int nrOfRabbits = 0;
            int nrOfPredators = 0;

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.Move();

                // Died of old age..
                if (!current.Alive)
                    continue;


                if (current.Specy == EntityType.Plant) nrOfPlants++;
                if (current.Specy == EntityType.Rabbit) nrOfRabbits++;
                if (current.Specy == EntityType.Predator) nrOfPredators++;

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

            Info.Content = string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
        }

        private void DrawObstacle(Placement obstacle)
        {
            DrawPolygon(obstacle.Form.Shape);
        }

        private void DrawPolygon(IPolygon polygon)
        {
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                DawnOnline.Simulation.Collision.Vector p1 = polygon.Points[i];
                DawnOnline.Simulation.Collision.Vector p2;
                if (i + 1 >= polygon.Points.Count)
                {
                    p2 = polygon.Points[0];
                }
                else
                {
                    p2 = polygon.Points[i + 1];
                }
                DrawLine(p1, p2);
            }
        }

        private void DrawCreature(ICreature creature)
        {
            var placement = creature.Place;

            // Body
            {
                var color = creature.Equals(_avatar) ? Brushes.Blue : Brushes.Red;

                //if (creature.SeesACreatureLeft())
                //    color = Brushes.LightGray;
                //if (creature.SeesACreatureRight())
                //    color = Brushes.LightGray;
                //if (creature.SeesACreatureForward())
                //    color = Brushes.Gray;

                if (creature.Specy == EntityType.Rabbit) color = Brushes.WhiteSmoke;
                if (creature.Specy == EntityType.Plant) color = Brushes.Green;

                var newEllipse = new Ellipse();
                newEllipse.Height = placement.Form.BoundingCircleRadius;
                newEllipse.Width = placement.Form.BoundingCircleRadius;
                newEllipse.Fill = color;
                newEllipse.SetValue(Canvas.LeftProperty, placement.Position.X - placement.Form.BoundingCircleRadius/2.0);
                newEllipse.SetValue(Canvas.TopProperty, placement.Position.Y - placement.Form.BoundingCircleRadius/2.0);
                newEllipse.Stroke = creature.IsTired ? Brushes.Blue : Brushes.Black;

                MyCanvas.Children.Add(newEllipse);
            }

            // Direction
            if (creature.Specy != EntityType.Plant)
            {
                var newLine = new Line();
                newLine.X1 = placement.Position.X;
                newLine.Y1 = placement.Position.Y;
                newLine.X2 = placement.Position.X + Math.Cos(placement.Angle) * placement.Form.BoundingCircleRadius;
                newLine.Y2 = placement.Position.Y + Math.Sin(placement.Angle) * placement.Form.BoundingCircleRadius;
                newLine.Stroke = Brushes.Black;

                MyCanvas.Children.Add(newLine);
            }

            // Shape
            //{
            //    for (int i = 0; i < creature.Place.Form.Shape.Points.Count; i++) 
            //    {
            //        var polygon = creature.Place.Form.Shape;
            //        DawnOnline.Simulation.Collision.Vector p1 = polygon.Points[i];
            //        DawnOnline.Simulation.Collision.Vector p2;
            //        if (i + 1 >= polygon.Points.Count) 
            //        {
            //            p2 = polygon.Points[0];
            //        } else {
            //            p2 = polygon.Points[i + 1];
            //        }
            //        DrawLine(p1, p2);
            //    }
            //}

             //LineOfSight
            //{
            //    foreach (var eye in creature.Eyes)
            //    {
            //        var lineOfSight = eye.GetLineOfSight();
            //        if (lineOfSight != null)
            //        {
            //            DrawPolygon(lineOfSight);
            //        }
            //    }
            //}
        }

        private void DrawLine(DawnOnline.Simulation.Collision.Vector p1, DawnOnline.Simulation.Collision.Vector p2)
        {
            var newLine = new Line();
            newLine.X1 = p1.X;
            newLine.Y1 = p1.Y;
            newLine.X2 = p2.X;
            newLine.Y2 = p2.Y;
            newLine.Stroke = Brushes.Black;

            MyCanvas.Children.Add(newLine);
        }

        private void DrawAll()
        {
            MyCanvas.Children.Clear();

            var creatures = _environment.GetCreatures();
            foreach (var current in creatures)
            {
                DrawCreature(current);
            }

            var obstacles = _environment.GetObstacles();
            foreach (var current in obstacles)
            {
                DrawObstacle(current.Place);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _environment.AddCreature(_avatar,
                                     new Vector2 { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) }, 0);

            BuildWorld();

            {
                var dt = new System.Windows.Threading.DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dt.Tick += UpdateClient;
                dt.Start();
            }

            {
                var dt = new System.Windows.Threading.DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 0, 0, 200);
                dt.Tick += MoveCreatures;
                dt.Start();
            }
        }

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(ObstacleBuilder.CreateObstacleBox(MaxX, -20), new Vector2(MaxX / 2.0f, -11)); // Top
            _environment.AddObstacle(ObstacleBuilder.CreateObstacleBox(MaxX, 20), new Vector2(MaxX / 2.0f, MaxY + 11)); // Bottom
            _environment.AddObstacle(ObstacleBuilder.CreateObstacleBox(-20, MaxY), new Vector2(-11, MaxY / 2.0f)); // Left
            _environment.AddObstacle(ObstacleBuilder.CreateObstacleBox(20, MaxY), new Vector2(MaxX + 11, MaxY / 2.0f)); // Right

            // Randow obstacles
            int maxHeight = 200;
            int maxWide = 200;
            for (int i=0; i < 50;)
            {
                int height = _randomize.Next(maxHeight);
                int wide = _randomize.Next(maxWide);
                var position = new Vector2(_randomize.Next(MaxX - wide), _randomize.Next(MaxY - height));
                var box = ObstacleBuilder.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        void UpdateClient(object sender, EventArgs e)
        {
            DrawAll();
        }

        void MoveCreatures(object sender, EventArgs e)
        {
            MoveAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((DateTime.Now - _lastMove).TotalMilliseconds > 50)
            {
                _lastMove = DateTime.Now;

                switch (e.Key)
                {
                    case Key.Up:
                        _avatar.WalkForward();
                        break;
                    case Key.Left:
                        _avatar.TurnLeft();
                        break;
                    case Key.Right:
                        _avatar.TurnRight();
                        break;
                }
            }
        }


        private double _currentScale = 1.0;
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _currentScale += (e.Delta > 0) ? 0.05 : -0.05;
            MyCanvas.LayoutTransform = new ScaleTransform(_currentScale, _currentScale);
        }

    }
}
