using System;
using DawnOnline.Simulation.Collision;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    internal class ActionQueue
    {
        internal Vector2 ForwardMotion { get; set; }
        internal double TurnMotion { get; set; }
        internal Vector2 StrafeMotion { get; set; }
        internal double FatigueCost { get; set; }
        internal double Damage { get; set; }
        internal bool Fire { get; set; }
        internal bool FireRocket { get; set; }
        internal bool HasAttacked { get; set; }
        internal bool HasFired { get; set; }
        internal DateTime LastAttackTime { get; set; }

        internal void ClearForRound()
        {
            FatigueCost = 0;
            TurnMotion = 0;
            StrafeMotion = Vector2.Zero;
            ForwardMotion = Vector2.Zero;
            HasAttacked = false;
            HasFired = false;
        }
    }
}
