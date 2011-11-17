using System;
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

        internal override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            //if (MyCreature.TryReproduce())
            //    return;

            //if (MyCreature.IsHungry)
            //{
            //    FindPlants();
            //}

            // Run from predator
            {
                if (_leftEye.SeesACreature(EntityType.Predator))
                {
                    //MyCreature.TurnRight();
                    MyCreature.RunForward();
                    return;
                }
                if (_rightEye.SeesACreature(EntityType.Predator))
                {
                    //MyCreature.TurnLeft();
                    MyCreature.RunForward();
                    return;
                }
            }



            if (MyCreature.IsTired)
            {
                MyCreature.RegisterRest();
                return;
            }


            FindPlants();

            DoRandomAction(100);
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
            if (_forwardEye.SeesACreature(EntityType.Plant))
            {
                MyCreature.RunForward();
                return;
            }
            if (_leftEye.SeesACreature(EntityType.Plant))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_rightEye.SeesACreature(EntityType.Plant))
            {
                MyCreature.TurnRight();
                return;
            }      
        }
    }
}
