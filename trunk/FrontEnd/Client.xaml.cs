﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Simulation;

namespace FrontEnd
{
    public partial class Window1 : Window
    {
        private readonly IEnvironment _environment = SimulationFactory.CreateEnvironment();
        private const int MaxX = 1000;
        private const int MaxY = 1000;
        private ICreature _avatar = SimulationFactory.CreateAvatar();
        private DateTime _lastMove = DateTime.Now;

        Random _randomize = new Random();

        public Window1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                AddCreature();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MoveAll();
        }

        private void MoveAll()
        {
            var creatures = new List<ICreature>(_environment.GetCreatures());

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.Move();

                var killed = current.Attack();
                if (killed != null)
                    _environment.KillCreature(killed);
            }
        }

        private void AddCreature()
        {
            _environment.AddCreature(SimulationFactory.CreateCreature(),
                             new Coordinate {X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY)},
                             _randomize.Next(6));
        }

        private void DrawCreature(ICreature creature)
        {
            var placement = creature.Place;

            // Body
            {
                var color = creature.Equals(_avatar) ? Brushes.Blue : Brushes.Red;

                if (creature.SeesACreatureLeft())
                    color = Brushes.LightGray;
                if (creature.SeesACreatureRight())
                    color = Brushes.LightGray;
                if (creature.SeesACreatureForward())
                    color = Brushes.Gray;

                var newEllipse = new Ellipse();
                newEllipse.Height = placement.Form.Radius;
                newEllipse.Width = placement.Form.Radius;
                newEllipse.Fill = color;
                newEllipse.SetValue(Canvas.LeftProperty, placement.Position.X - placement.Form.Radius/2.0);
                newEllipse.SetValue(Canvas.TopProperty, placement.Position.Y - placement.Form.Radius/2.0);
                newEllipse.Stroke = Brushes.Black;

                MyCanvas.Children.Add(newEllipse);
            }

            // Direction
            {
                var newLine = new Line();
                newLine.X1 = placement.Position.X;
                newLine.Y1 = placement.Position.Y;
                newLine.X2 = placement.Position.X + Math.Cos(placement.Angle) * placement.Form.Radius;
                newLine.Y2 = placement.Position.Y + Math.Sin(placement.Angle) * placement.Form.Radius;
                newLine.Stroke = Brushes.Black;

                MyCanvas.Children.Add(newLine);
            }

        }

        private void DrawAll()
        {
            MyCanvas.Children.Clear();

            var creatures = _environment.GetCreatures();
            foreach (var current in creatures)
            {
                DrawCreature(current);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _environment.AddCreature(_avatar,
                                     new Coordinate { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) }, 0);

            {
                var dt = new System.Windows.Threading.DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
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
    }
}
