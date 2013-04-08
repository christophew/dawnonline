using System.Diagnostics;
using System;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.AgentMatrix.Brains
{
    internal class PredatorBrain2 : AbstractBrain
    {
        private IEye _forwardEye;
        private IEye _leftEye;
        private IEye _rightEye;
        private bool _initialized;

        public override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Find something to attack
            var creatureToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creatureToAttack != null)
            {
                MyCreature.Attack();
                return;
            }

            // Move
            if (_forwardEye.SeesACreature(MyCreature.FoodSpecies))
            {
                MyCreature.RunForward();
                return;
            }
            if (_leftEye.SeesACreature(MyCreature.FoodSpecies))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_rightEye.SeesACreature(MyCreature.FoodSpecies))
            {
                MyCreature.TurnRight();
                return;
            }

            //if (MyCreature.TryReproduce())
            //    return;

            if (MyCreature.IsTired)
            {
                MyCreature.Rest();
                return;
            }

            DoRandomAction(100);
        }

        public override void InitializeSenses()
        {
            _forwardEye = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _leftEye = SensorBuilder.CreateEye(MyCreature, -MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _rightEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(60), MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);

            _initialized = true;
        }
    }
}
