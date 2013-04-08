using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.AgentMatrix.Brains
{
    class TurretBrain : AbstractBrain
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
            if (_forwardEye.SeesACreature(MyCreature.FoodSpecies))
            {
                //MyCreature.Fire();
                MyCreature.FireRocket();
            }

            // Turn
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

            // Fallback
            //MyCreature.TurnLeft();
        }

        public override void InitializeSenses()
        {
            _forwardEye = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(10), MyCreature.CharacterSheet.VisionDistance);
            _leftEye = SensorBuilder.CreateEye(MyCreature, -MathTools.ConvertToRadials(90), MathTools.ConvertToRadials(90), MyCreature.CharacterSheet.VisionDistance);
            _rightEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(90), MathTools.ConvertToRadials(90), MyCreature.CharacterSheet.VisionDistance);

            _initialized = true;
        }

    }
}
