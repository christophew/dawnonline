using System;

namespace DawnOnline.Simulation
{
    internal static class Globals
    {
        private static readonly Random _randomize = new Random();
        public static Random Radomizer
        {
            get { return _randomize; }
        }

        private static int _currentIdCounter = 1;
        public static int GenerateUniqueId()
        {
            return _currentIdCounter++;
        }
    }
}
