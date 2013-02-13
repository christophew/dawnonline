using System;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.Simulation
{
    public class ActionQueue
    {
        public Vector2 ForwardMotion { get; internal set; }
        public double TurnMotion { get; internal set; }
        public Vector2 StrafeMotion { get; internal set; }
        public double FatigueCost { get; internal set; }
        public double Damage { get; internal set; }
        public bool Fire { get; internal set; }
        public bool FireRocket { get; internal set; }
        public bool Attack { get; internal set; }
        public bool HasAttacked { get; internal set; }
        public bool HasFired { get; internal set; }
        public EntityType BuildEntityOfType { get; internal set; }
        public bool Rest { get; internal set; }

        public bool RegisterSpawn { get; internal set; }

        // TEMP for client/server sync
        public double ForwardThrustPercent { get; internal set; }
        public double TurnPercent { get; internal set; }

        // Sounds
        public double SpeachVolumeA { get; internal set; }
        public double SpeachVolumeB { get; internal set; }

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
            RegisterSpawn = false; // Needs to be reset on the AgentMatrix, but should be reset on usage on the Server
            Attack = false;
            SpeachVolumeA = 0;
            SpeachVolumeB = 0;

            // TEMP for client/server sync
            ForwardThrustPercent = 0;
            TurnPercent = 0;
        }
    }
}
