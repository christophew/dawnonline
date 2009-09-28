using System;

namespace DawnOnline.Simulation
{
    static class Globals
    {
        private static readonly Random _randomize = new Random();
        public static Random Radomizer { get { return _randomize; } }
    }
}
