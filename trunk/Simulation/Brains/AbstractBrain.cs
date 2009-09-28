using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Brains
{
    internal abstract class AbstractBrain
    {
        internal Creature MyCreature { get; set; }

        internal abstract void DoSomething();

        protected void DoRandomAction()
        {
            int randomAction = Globals.Radomizer.Next(4);

            if (randomAction == 0)
                MyCreature.WalkForward();
            if (randomAction == 1)
                MyCreature.TurnLeft();
            if (randomAction == 2)
                MyCreature.TurnRight();
            if (randomAction == 3)
                MyCreature.Rest();
        }
    }
}
