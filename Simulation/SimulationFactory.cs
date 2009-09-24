using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
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

        public static ICreature CreateCreature()
        {
            var critter = new Creature();
            critter.HasBrain = true;
            return critter;
        }

        public static ICreature CreateAvatar()
        {
            var avatar = new Creature();
            avatar.HasBrain = false;
            return avatar;
        }
    }
}
