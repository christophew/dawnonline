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
        internal bool HasAttacked { get; set; }
    }
}
