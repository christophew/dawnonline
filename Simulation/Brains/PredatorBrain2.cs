using System.Diagnostics;
using System;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain2 : AbstractBrain
    {
        private Eye _forwardEye;
        private Eye _leftEye;
        private Eye _rightEye;
        private bool _initialized;

        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecy);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
                return;
            }

            // Move
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && _forwardEye.SeesACreature(MyCreature.FoodSpecy))
            {
                MyCreature.RunForward();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && _leftEye.SeesACreature(MyCreature.FoodSpecy))
            {
                MyCreature.TurnLeft();
                return;
            }
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && _rightEye.SeesACreature(MyCreature.FoodSpecy))
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
    }
}
