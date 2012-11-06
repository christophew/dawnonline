using System;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

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
        internal EntityType BuildEntityOfType { get; set; }
        internal bool Rest { get; set; }

        // Sounds
        internal double SpeachVolumeA { get; set; }
        internal double SpeachVolumeB { get; set; }

        // Timers: not cleared
        internal DateTime LastAttackTime { get; set; }
        internal DateTime LastBuildTime { get; set; }
        internal DateTime LastRestTime { get; set; }

        internal void ClearForRound()
        {
            FatigueCost = 0;
            TurnMotion = 0;
            StrafeMotion = Vector2.Zero;
            ForwardMotion = Vector2.Zero;
            HasAttacked = false;
            HasFired = false;
            BuildEntityOfType = EntityType.Unknown;
            Rest = false;
            SpeachVolumeA = 0;
            SpeachVolumeB = 0;
        }

    }
}
