using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    internal abstract class AbstractBrain : ICloneable
    {
        internal Creature MyCreature { get; set; }

        internal abstract void DoSomething();

        protected void DoRandomAction()
        {
            int randomAction = Globals.Radomizer.Next(5);

            if (randomAction == 0)
                MyCreature.WalkForward();
            if (randomAction == 1)
                MyCreature.TurnLeft();
            if (randomAction == 2)
                MyCreature.TurnRight();
            if (randomAction == 3)
                MyCreature.Rest();
            if (randomAction == 4)
                MyCreature.WalkForward();
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
