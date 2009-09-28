using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;

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
            return new Form { Radius = radius };
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
