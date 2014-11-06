using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Statistics
{
    public class ScoreStatistics
    {
        public double ResourcesDelivered { get; internal set; }
        public double ResourcesGathered { get; internal set; }

        public int NrOfTimesResourcesDelivered { get; internal set; }
        public int NrOfSpawns { get; internal set; }
        public int NrOfOwnCreaturesKilled { get; internal set; }

        public double DamageDone { get; internal set; }
        public double DamageReceived { get; internal set; }
    }
}
