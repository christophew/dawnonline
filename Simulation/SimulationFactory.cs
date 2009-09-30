using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    public static class SimulationFactory
    {
        public static IEnvironment CreateEnvironment()
        {
            return new Environment();
        }

        public static IForm CreateCircle(double radius)
        {
            float halfRadius = (float)(radius / 2.0);
            Polygon box = new Polygon();
            box.Points.Add(new Vector(halfRadius, halfRadius));
            box.Points.Add(new Vector(-halfRadius, halfRadius));
            box.Points.Add(new Vector(-halfRadius, -halfRadius));
            box.Points.Add(new Vector(halfRadius, -halfRadius));
            box.BuildEdges();

            return new Form { Radius = radius, Shape = box };
        }

        public static IPlacement CreateObstacleBox(double deltaX, double deltaY)
        {
            double radius = Math.Max(deltaX, deltaY);

            float halfDeltaX = (float)(deltaX / 2.0);
            float halfDeltaY = (float)(deltaY / 2.0);
            Polygon box = new Polygon();
            box.Points.Add(new Vector(halfDeltaX, halfDeltaY));
            box.Points.Add(new Vector(-halfDeltaX, halfDeltaY));
            box.Points.Add(new Vector(-halfDeltaX, -halfDeltaY));
            box.Points.Add(new Vector(halfDeltaX, -halfDeltaY));
            box.BuildEdges();

            var form = new Form { Radius = radius, Shape = box };
            var placement = new Placement {Form = form};

            return placement;
        }

        public static ICreature CreatePredator()
        {
            var critter = new Creature(15);
            critter.Brain = new PredatorBrain();

            critter.Statistics.WalkingDistance = 5;
            critter.Statistics.TurningAngle = 0.2;

            return critter;
        }

        public static ICreature CreateRabbit()
        {
            var critter = new Creature(10);
            critter.Brain = new RabbitBrain();

            critter.Statistics.WalkingDistance = 4;
            critter.Statistics.TurningAngle = 0.35;

            return critter;
        }

        public static ICreature CreateAvatar()
        {
            var avatar = new Creature(15);

            avatar.Statistics.WalkingDistance = 6;
            avatar.Statistics.TurningAngle = 0.35;

            return avatar;
        }
    }
}
