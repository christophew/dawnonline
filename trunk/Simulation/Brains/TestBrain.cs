using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Brains
{
    internal class TestBrain : AbstractBrain
    {
        internal override void DoSomething(TimeSpan timeDelta)
        {
            MyCreature.WalkForward();
            MyCreature.TurnRightSlow();
        }
    }
}
