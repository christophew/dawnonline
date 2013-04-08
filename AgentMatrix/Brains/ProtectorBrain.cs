using System.Diagnostics;
using System;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;

namespace DawnOnline.AgentMatrix.Brains
{
    internal class ProtectorBrain : PredatorBrain
    {
        private IEye _backwardEye;

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

        protected override void NeutralState(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Return to base
            var spawnPoint = MyCreature.SpawnPoint as ICreature;
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

        public override void InitializeSenses()
        {
            // Eyes
            _forwardEye = SensorBuilder.CreateEye(MyCreature, 0.0, MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _backwardEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(180), MathTools.ConvertToRadials(30), MyCreature.CharacterSheet.VisionDistance);
            _leftEye = SensorBuilder.CreateEye(MyCreature, -MathTools.ConvertToRadials(90), MathTools.ConvertToRadials(75), MyCreature.CharacterSheet.VisionDistance);
            _rightEye = SensorBuilder.CreateEye(MyCreature, MathTools.ConvertToRadials(90), MathTools.ConvertToRadials(75), MyCreature.CharacterSheet.VisionDistance);

            // Bumpers
            _forwardBumper = SensorBuilder.CreateBumper(MyCreature, new Vector2(15, 0));

            _initialized = true;
        }

        public override void ClearState()
        {
            _forwardBumper.Clear();
        }
    }
}
