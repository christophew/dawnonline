﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.Simulation.Brains
{
    class TurretBrain : AbstractBrain
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
            if ((Globals.Radomizer.Next(100) < MyCreature.CharacterSheet.VisionAccuracyPercent) && _forwardEye.SeesACreature(MyCreature.FoodSpecy))
            {
                MyCreature.Fire();
            }

            // Turn
            if (_leftEye.SeesACreature(MyCreature.FoodSpecy))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_rightEye.SeesACreature(MyCreature.FoodSpecy))
            {
                MyCreature.TurnRight();
                return;
            }

            // Fallback
            //MyCreature.TurnLeft();
        }

        internal override void InitializeSenses()
        {
            _forwardEye = new Eye(MyCreature)
            {
                Angle = 0.0,
                VisionAngle = MathTools.ConvertToRadials(10),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _leftEye = new Eye(MyCreature)
            {
                Angle = -MathTools.ConvertToRadials(90),
                VisionAngle = MathTools.ConvertToRadials(85),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _rightEye = new Eye(MyCreature)
            {
                Angle = MathTools.ConvertToRadials(90),
                VisionAngle = MathTools.ConvertToRadials(85),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };

            _initialized = true;
        }

    }
}
