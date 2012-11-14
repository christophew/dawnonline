﻿using System.Diagnostics;
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

        internal override void DoSomething(TimeSpan timeDelta)
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
                MyCreature.RegisterRest();
                return;
            }

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
    }
}