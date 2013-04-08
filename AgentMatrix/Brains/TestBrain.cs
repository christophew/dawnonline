using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.AgentMatrix.Brains
{
    internal class TestBrain : AbstractBrain
    {
        public override void DoSomething(TimeSpan timeDelta)
        {
            MyCreature.WalkForward();
            MyCreature.TurnRightSlow();
        }
    }
}
