﻿using System;
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

        //internal int MaxAge { get; set; }
        internal double FoodValue { get; set; }

        // Monitors
        internal Monitor Fatigue { get; private set; }
        internal Monitor Hunger { get; private set; }

        internal CharacterSheet()
        {
            Fatigue = new Monitor();
            Hunger = new Monitor(200);

            // Defaults
            WalkingDistance = 1;
            TurningAngle = 0.01;
            VisionAccuracyPercent = 70;
            VisionDistance = 250;
            FatigueCost = 15;
            FatigueRecovery = 30;
            MeleeRange = 15;
            //MaxAge = Int32.MaxValue;

            FoodValue = 10;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}