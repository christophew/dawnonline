using System;
using System.Diagnostics;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Brains
{
    internal class RabbitBrain : AbstractBrain
    {
        private IEye _forwardEye;
        private IEye _leftEye;
        private IEye _rightEye;
        private bool _initialized;

        public override void DoSomething(TimeSpan timeDelta)
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
                MyCreature.Rest();
                return;
            }


            FindPlants();

            DoRandomAction(100);
        }

        public override void InitializeSenses()
        {
            _forwardEye = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _leftEye = SensorBuilder.CreateEye(MyCreature, -MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _rightEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);

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
