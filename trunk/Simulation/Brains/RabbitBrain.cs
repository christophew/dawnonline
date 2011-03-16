using System.Diagnostics;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.Brains
{
    internal class RabbitBrain : AbstractBrain
    {
        private Eye _forwardEye;
        private Eye _leftEye;
        private Eye _rightEye;
        private bool _initialized;

        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            if (MyCreature.TryReproduce())
                return;

            if (MyCreature.IsHungry)
            {
                FindPlants();
            }

            // Run from predator
            {
                if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                    _leftEye.SeesACreature(CreatureType.Predator))
                {
                    //MyCreature.TurnRight();
                    MyCreature.RunForward();
                    return;
                }
                if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                    _rightEye.SeesACreature(CreatureType.Predator))
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

        internal override void InitializeSenses()
        {
            _forwardEye = new Eye(MyCreature)
            {
                Angle = 0.0,
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _leftEye = new Eye(MyCreature)
            {
                Angle = -MathTools.ConvertToRadials(60),
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _rightEye = new Eye(MyCreature)
            {
                Angle = MathTools.ConvertToRadials(60),
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };

            _initialized = true;
        }

        private void FindPlants()
        {
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && _forwardEye.SeesACreature(CreatureType.Plant))
            {
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                _leftEye.SeesACreature(CreatureType.Plant))
            {
                MyCreature.TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) &&
                _rightEye.SeesACreature(CreatureType.Plant))
            {
                MyCreature.TurnRight();
                return;
            }      
        }
    }
}
