using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Statistics
{
    internal class CharacterSheet
    {
        internal double WalkingDistance { get; set; }
        internal double RunningDistance { get { return WalkingDistance*2.0; } }
        internal double TurningAngle { get; set; }
        internal int VisionAccuracyPercent { get; set; }
        internal double VisionRadius { get; set; }
        internal double FatigueCost { get; set; }
        internal double FatigueRecovery { get; set; }

        // Monitors
        internal Monitor Fatigue { get; private set; }

        internal CharacterSheet()
        {
            Fatigue = new Monitor();

            // Defaults
            WalkingDistance = 1;
            TurningAngle = 0.01;
            VisionAccuracyPercent = 70;
            VisionRadius = 40;
            FatigueCost = 15;
            FatigueRecovery = 30;
        }
    }
}