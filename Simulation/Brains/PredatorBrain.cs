using System.Diagnostics;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        private Creature FindCreatureToAttack()
        {
            var creaturesToAttack = MyCreature.MyEnvironment.GetCreaturesInRange(MyCreature.Place.Position,
                                                                                 MyCreature.CharacterSheet.MeleeRange,
                                                                                 MyCreature.FoodSpecy);

            foreach (Creature current in creaturesToAttack)
            {
                if (!current.Equals(MyCreature))
                    return current;
            }
            return null;
        }

        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);

            // Find something to attack
            var creaturesToAttack = FindCreatureToAttack();
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
                return;
            }

            // Move
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && MyCreature.SeesACreatureForward(MyCreature.FoodSpecy))
            {
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && MyCreature.SeesACreatureLeft(MyCreature.FoodSpecy))
            {
                MyCreature.TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && MyCreature.SeesACreatureRight(MyCreature.FoodSpecy))
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
