using System;
using System.Diagnostics;

namespace DawnOnline.AgentMatrix
{
    internal static class Globals
    {
        private static readonly Random _randomize = new Random();
        public static Random Radomizer
        {
            get { return _randomize; }
        }
    }
}
