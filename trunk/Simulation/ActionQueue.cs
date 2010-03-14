using System;
using DawnOnline.Simulation.Collision;

namespace DawnOnline.Simulation
{
    internal class ActionQueue : IActionQueue
    {
        public Vector ForwardMotion { get; set; }
        public double TurnMotion { get; set; }
        public double FatigueCost { get; set; }
        public double Damage { get; set; }
        public bool HasAttacked { get; set; }
    }
}
