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
        public double RangeDamage { get; internal set; }

        public double Armour { get; internal set; }

        public bool UseAutoAttack { get; internal set; }

        public double AttackCoolDown { get; internal set; }
        public double BuildCoolDown { get; internal set; }
        public double RestCoolDown { get; internal set; }
        public double RegenCoolDown { get; internal set; }

        internal double FoodValue { get; set; }

        // Personal during livetime
        public double Score { get; internal set; }
        public int Generation { get; internal set; }

        // Monitors
        public Monitor Fatigue { get; internal set; }
        public Monitor Damage { get; internal set; }
        public Monitor Resource { get; internal set; }

        // Regen
        public double FatigueRegen { get; internal set; }
        public double HealthRegen { get; internal set; }

        // Prototype Persistency layer
        public void Restore(double score, int generation)
        {
            Score = score;
            Generation = generation;
        }


        internal CharacterSheet()
        {
            Fatigue = new Monitor();
            Damage = new Monitor();
            Resource = new Monitor();

            // Defaults
            WalkingDistance = 10;
            TurningAngle = 1;
            VisionDistance = 25.0;
            FatigueCost = 15;
            FatigueRecovery = 30;
            MeleeRange = 2.0;
            MeleeDamage = 25;

            FoodValue = 10;

            AttackCoolDown = 0.2; // Seconds
            BuildCoolDown = 5; // Seconds
            RestCoolDown = 1; // Seconds
            RegenCoolDown = 2; // Seconds

            FatigueRegen = 1;
            HealthRegen = 0;
        }

        internal CharacterSheet Replicate()
        {
            var newCharacterSheet = new CharacterSheet();

            newCharacterSheet.WalkingDistance = WalkingDistance;
            newCharacterSheet.TurningAngle = TurningAngle;
            newCharacterSheet.VisionDistance = VisionDistance;
            newCharacterSheet.FatigueCost = FatigueCost;
            newCharacterSheet.FatigueRecovery = FatigueRecovery;
            newCharacterSheet.MeleeRange = MeleeRange;
            newCharacterSheet.MeleeDamage = MeleeDamage;
            newCharacterSheet.RangeDamage = RangeDamage;

            newCharacterSheet.AttackCoolDown = AttackCoolDown;
            newCharacterSheet.BuildCoolDown = BuildCoolDown;
            newCharacterSheet.RestCoolDown = RestCoolDown;
            newCharacterSheet.RegenCoolDown = RegenCoolDown;

            newCharacterSheet.FoodValue = FoodValue;

            newCharacterSheet.FatigueRegen = FatigueRegen;
            newCharacterSheet.HealthRegen = HealthRegen;

            // Increase generation
            newCharacterSheet.Generation = Generation + 1;

            return newCharacterSheet;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}