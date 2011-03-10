using System;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    internal class ActionQueue
    {
        internal Vector ForwardMotion { get; set; }
        internal double TurnMotion { get; set; }
        internal double FatigueCost { get; set; }
        internal double Damage { get; set; }
        internal bool Fire { get; set; }
        internal bool HasAttacked { get; set; }
        internal bool HasFired { get; set; }
        internal DateTime LastAttackTime { get; set; }

        internal void ClearForRound()
        {
            FatigueCost = 0;
            TurnMotion = 0;
            ForwardMotion = new Vector();
            HasAttacked = false;
            HasFired = false;
        }
    }
}
