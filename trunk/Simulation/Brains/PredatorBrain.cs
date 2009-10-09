using System.Diagnostics;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);

            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && MyCreature.SeesACreatureForward(CreatureType.Rabbit))
            {
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && MyCreature.SeesACreatureLeft(CreatureType.Rabbit))
            {
                MyCreature.TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && MyCreature.SeesACreatureRight(CreatureType.Rabbit))
            {
                MyCreature.TurnRight();
                return;
            }

            if (MyCreature.TryReproduce())
                return;

            if (MyCreature.IsTired)
            {
                MyCreature.Rest();
                return;
            }

            DoRandomAction();
        }

    }
}
