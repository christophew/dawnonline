using System.Diagnostics;
using System;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);

            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecy);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
                return;
            }

            // Move
            if (MyCreature.SeesACreatureForward(MyCreature.FoodSpecy))
            {
                MyCreature.RunForward();
                return;
            }
            if (MyCreature.SeesACreatureLeft(MyCreature.FoodSpecy))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (MyCreature.SeesACreatureRight(MyCreature.FoodSpecy))
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
