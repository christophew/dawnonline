using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Statistics
{
    public class CharacterSheet : ICloneable
    {
        public double WalkingDistance { get; internal set; }
        public double RunningDistance { get { return WalkingDistance * 2.0; } }
        public double TurningAngle { get; internal set; }
        public double VisionDistance { get; internal set; }
        public double FatigueCost { get; internal set; }
        public double FatigueRecovery { get; internal set; }
        public double MeleeRange { get; internal set; }
        public double MeleeDamage { get; internal set; }
        public double CoolDown { get; internal set; }
        public double RangeDamage { get; internal set; }

        internal int VisionAccuracyPercent { get; set; }

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
            MeleeRange = 20;
            MeleeDamage = 25;
            //MaxAge = Int32.MaxValue;
            ReproductionIncreaseAverage = 5;

            FoodValue = 10;
            CoolDown = 0.2; // Seconds
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}