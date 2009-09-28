using System.Diagnostics;

namespace DawnOnline.Simulation.Brains
{
    internal class RabbitBrain : AbstractBrain
    {
        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);

            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && !MyCreature.SeesACreatureForward())
            {
                MyCreature.WalkForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && MyCreature.SeesACreatureLeft())
            {
                MyCreature.TurnRight();
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.Statistics.VisionAccuracyPercent) && MyCreature.SeesACreatureRight())
            {
                MyCreature.TurnLeft();
                MyCreature.RunForward();
                return;
            }

            if (MyCreature.IsTired)
            {
                MyCreature.Rest();
                return;
            }

            DoRandomAction();
        }
    }
}
