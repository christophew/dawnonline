using System.Diagnostics;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    internal class RabbitBrain : AbstractBrain
    {
        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);

            if (MyCreature.TryReproduce())
                return;

            if (MyCreature.IsHungry)
            {
                FindPlants();
            }

            // Run from predator
            {
                if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                    MyCreature.SeesACreatureLeft(CreatureType.Predator))
                {
                    //MyCreature.TurnRight();
                    MyCreature.RunForward();
                    return;
                }
                if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                    MyCreature.SeesACreatureRight(CreatureType.Predator))
                {
                    //MyCreature.TurnLeft();
                    MyCreature.RunForward();
                    return;
                }
            }



            if (MyCreature.IsTired)
            {
                MyCreature.Rest();
                return;
            }


            FindPlants();

            DoRandomAction();
        }

        private void FindPlants()
        {
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && MyCreature.SeesACreatureForward(CreatureType.Plant))
            {
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                MyCreature.SeesACreatureLeft(CreatureType.Plant))
            {
                MyCreature.TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                MyCreature.SeesACreatureRight(CreatureType.Plant))
            {
                MyCreature.TurnRight();
                return;
            }      
        }
    }
}
