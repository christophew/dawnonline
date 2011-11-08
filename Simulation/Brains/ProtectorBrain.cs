using System.Diagnostics;
using System;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Brains
{
    internal class ProtectorBrain : PredatorBrain
    {
        private Eye _backwardEye;

        protected override bool ISeeAnEnemy()
        {
            var check = _forwardEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint) ||
                        _leftEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint) ||
                        _rightEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint) ||
                        _backwardEye.SeesACreature(MyCreature.FoodSpecies, MyCreature.SpawnPoint);

            if (check)
                _lastTimeISawAnEnemy = DateTime.Now;
            return check;
        }

        protected override void NeutralState()
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Return to base
            var spawnPoint = MyCreature.SpawnPoint as Creature;
            if (_forwardEye.SeesCreature(spawnPoint))
            {
                MyCreature.RunForward();
                return;
            }

            // Try to walk backward to base
            if (_leftEye.SeesCreature(spawnPoint))
            {
                MyCreature.TurnRight();
                return;
            }
            if (_rightEye.SeesCreature(spawnPoint))
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_backwardEye.SeesCreature(spawnPoint))
            {
                MyCreature.WalkBackward();
                return;
            }

            // Maybe we hit a base?
            if (_forwardBumper.Hit)
            {
                MyCreature.TurnRight();
                return;
            }

            DoRandomAction(500);
        }

        internal override void InitializeSenses()
        {
            // Eyes
            _forwardEye = new Eye(MyCreature)
            {
                Angle = 0.0,
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _backwardEye = new Eye(MyCreature)
            {
                Angle = MathTools.ConvertToRadials(180),
                VisionAngle = MathTools.ConvertToRadials(30),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _leftEye = new Eye(MyCreature)
            {
                Angle = -MathTools.ConvertToRadials(90),
                VisionAngle = MathTools.ConvertToRadials(75),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };
            _rightEye = new Eye(MyCreature)
            {
                Angle = MathTools.ConvertToRadials(90),
                VisionAngle = MathTools.ConvertToRadials(75),
                VisionDistance = MyCreature.CharacterSheet.VisionDistance
            };

            // Bumpers
            _forwardBumper = new Bumper(MyCreature, new Vector2(15, 0));


            _initialized = true;
        }

        internal override void ClearState()
        {
            _forwardBumper.Clear();
        }
    }
}
