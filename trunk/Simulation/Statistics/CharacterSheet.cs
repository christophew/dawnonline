using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Statistics
{
    internal class CharacterSheet : ICloneable
    {
        internal double WalkingDistance { get; set; }
        internal double RunningDistance { get { return WalkingDistance*2.0; } }
        internal double TurningAngle { get; set; }
        internal int VisionAccuracyPercent { get; set; }
        internal double VisionDistance { get; set; }
        internal double FatigueCost { get; set; }
        internal double FatigueRecovery { get; set; }
        internal double MeleeRange { get; set; }

        // Monitors
        internal Monitor Fatigue { get; private set; }

        internal CharacterSheet()
        {
            Fatigue = new Monitor();

            // Defaults
            WalkingDistance = 1;
            TurningAngle = 0.01;
            VisionAccuracyPercent = 70;
            VisionDistance = 100;
            FatigueCost = 15;
            FatigueRecovery = 30;
            MeleeRange = 15;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}