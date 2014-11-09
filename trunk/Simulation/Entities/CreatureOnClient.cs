using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    internal class CreatureOnClient : Creature
    {
        internal CreatureOnClient(double bodyRadius) : base(bodyRadius)
        {}

        internal override Creature CreateCreature(double radius)
        {
            return new CreatureOnClient(_place.Radius);
        }

        public override void Think(TimeSpan timeDelta)
        {
            Debug.Assert(Alive);

            if (!HasBrain)
                return;

            // Clear action queue: the brain will select new actions
            ClearActionQueue();

            Brain.DoSomething(timeDelta);
            Brain.ClearState();
        }
    }
}
