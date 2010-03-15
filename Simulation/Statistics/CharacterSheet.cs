﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Statistics
{
    public class CharacterSheet : ICloneable
    {
        internal double WalkingDistance { get; set; }
        internal double RunningDistance { get { return WalkingDistance * 2.0; } }
        internal double TurningAngle { get; set; }
        internal int VisionAccuracyPercent { get; set; }
        internal double VisionDistance { get; set; }
        internal double FatigueCost { get; set; }
        internal double FatigueRecovery { get; set; }
        internal double MeleeRange { get; set; }
        internal double MeleeDamage { get; set; }

        //internal int MaxAge { get; set; }
        internal double FoodValue { get; set; }

        internal int ReproductionIncreaseAverage { get; set; }


        // Monitors
        public Monitor Fatigue { get; internal set; }
        public Monitor Damage { get; internal set; }
        internal Monitor Hunger { get; set; }
        internal Monitor Reproduction { get; set; }

        internal CharacterSheet()
        {
            Fatigue = new Monitor();
            Damage = new Monitor();
            Hunger = new Monitor(500);
            Reproduction = new Monitor(1000);

            // Defaults
            WalkingDistance = 10;
            TurningAngle = 1;
            VisionAccuracyPercent = 70;
            VisionDistance = 250;
            FatigueCost = 15;
            FatigueRecovery = 30;
            MeleeRange = 15;
            MeleeDamage = 25;
            //MaxAge = Int32.MaxValue;
            ReproductionIncreaseAverage = 5;

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