using System;
using System.Diagnostics;

namespace DawnOnline.Simulation
{
    internal static class Globals
    {
        private static readonly Random _randomize = new Random();
        public static Random Radomizer
        {
            get { return _randomize; }
        }

        private static int _instanceId = -1;
        public static void SetInstanceId(int id)
        {
            Debug.Assert(_instanceId == -1, "Only set once");
            Debug.Assert(_instanceId < 1000, "Not supported: see GenerateUniqueId");
            _instanceId = id;
        }

        public static int GetInstanceId()
        {
            return _instanceId;
        }

        public static bool IsServerInstance()
        {
            // TO VERIFY: WHY?
            return _instanceId == 0;
        }

        private static int _currentIdCounter = 1;
        public static int GenerateUniqueId()
        {
            Debug.Assert(_instanceId != -1);

            return _currentIdCounter++ * 1000 + _instanceId;
        }
    }
}
