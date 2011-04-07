using System.Diagnostics;
using System;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        private Eye _forwardEye;
        private Eye _leftEye;
        private Eye _rightEye;
        private Bumper _forwardBumper;
        private bool _initialized;

        private enum EvadeState
        {
            None,
            Left,
            Right
        } ;
        private EvadeState _evading = EvadeState.None;
        private DateTime _startEvading;

        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
                MyCreature.WalkForward();
                return;
            }

            // Move
            if (_forwardEye.SeesACreature(MyCreature.FoodSpecies))
            {
                MyCreature.RunForward();
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

            if (MyCreature.TryReproduce())
                return;

            if (MyCreature.IsTired)
            {
                MyCreature.Rest();
                return;
            }

            // Turn on bumper-hit
            if (_forwardBumper.Hit)
            {
                if (_evading == EvadeState.None)
                {
                    // Chose left or right
                    _evading = Globals.Radomizer.Next(2) == 0 ? EvadeState.Left : EvadeState.Right;
                    _startEvading = DateTime.Now;
                }

                if (_evading == EvadeState.Left)
                    MyCreature.TurnLeft();
                if (_evading == EvadeState.Right)
                    MyCreature.TurnRight();
                return;
            }

            // Reset evade direction x-seconds after evade
            if (_evading != EvadeState.None)
            {
                if ((DateTime.Now - _startEvading).TotalSeconds > 5)
                    _evading = EvadeState.None;
            }

            DoRandomAction();
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
            _leftEye = new Eye(MyCreature)
                           {
                               Angle = -MathTools.ConvertToRadials(60),
                               VisionAngle = MathTools.ConvertToRadials(60),
                               VisionDistance = MyCreature.CharacterSheet.VisionDistance
                           };
            _rightEye = new Eye(MyCreature)
                            {
                                Angle = MathTools.ConvertToRadials(60),
                                VisionAngle = MathTools.ConvertToRadials(60),
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
