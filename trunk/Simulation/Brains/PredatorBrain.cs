using System.Collections.Generic;
using System.Diagnostics;
using System;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;
using System.Linq;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        protected Eye _forwardEye;
        protected Eye _leftEye;
        protected Eye _rightEye;
        protected Bumper _forwardBumper;
        protected bool _initialized;

        protected Dictionary<Eye, double> _eyeSee = new Dictionary<Eye, double>();


        protected enum EvadeState
        {
            None,
            Left,
            Right
        } ;
        protected EvadeState _evading = EvadeState.None;
        protected DateTime _startEvading;

        protected DateTime _lastTimeISawAnEnemy = new DateTime();

        protected DateTime? _imPanickingSince = null;


        internal override void DoSomething()
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Fill eye buffer
            See();

            // Emotional states
            // * neutral
            // * adrenaline
            // * fear
            // * tired

            if (MyCreature.CharacterSheet.Damage.IsCritical &&
                (!_imPanickingSince.HasValue || ((DateTime.Now - _imPanickingSince.Value).Milliseconds > 5000)))
            {
                FearState();
                return;
            }
            if ((DateTime.Now - _lastTimeISawAnEnemy).TotalMilliseconds < 2000 || ISeeAnEnemy())
            {
                AdrenalineState();
                return;
            }
            if (MyCreature.CharacterSheet.Fatigue.PercentFilled != 0)
            {
                TiredState();
                return;
            }

            NeutralState();
        }

        protected virtual bool ISeeAnEnemy()
        {
            var check = _eyeSee[_forwardEye] > 0 || _eyeSee[_leftEye] > 0 || _eyeSee[_rightEye] > 0;

            if (check)
                _lastTimeISawAnEnemy = DateTime.Now;
            return check;
        }

        protected virtual void NeutralState()
        {
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

            DoRandomAction(500);
        }

        protected virtual void AdrenalineState()
        {
            // Find something to attack
            var creaturesToAttack = MyCreature.FindCreatureToAttack(MyCreature.FoodSpecies);
            if (creaturesToAttack != null)
            {
                MyCreature.Attack(creaturesToAttack);
                MyCreature.WalkForward();
                return;
            }

            // Move
            if (_eyeSee[_forwardEye] > 0)
            {
                MyCreature.RunForward();
            }
            if (_eyeSee[_leftEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_eyeSee[_rightEye] > 0)
            {
                MyCreature.TurnRight();
                return;
            }

            // Circle target
            if (_forwardBumper.Hit)
            {
                MyCreature.TurnLeft();
                return;
            }


            // Where is he?
            DoRandomAction(250);
        }

        protected virtual void FearState()
        {
            if (!_imPanickingSince.HasValue)
                _imPanickingSince = DateTime.Now;

            // Run away!!!
            if (_eyeSee[_forwardEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_eyeSee[_leftEye] > 0)
            {
                MyCreature.TurnRight();
                return;
            }
            if (_eyeSee[_rightEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }

            // All clear!!
            MyCreature.RunForward();
        }

        protected virtual void TiredState()
        {
            // Rest
            MyCreature.RegisterRest();
        }

        protected void See()
        {
            var creatures = MyCreature.MyEnvironment.GetCreatures(MyCreature.FoodSpecies);

            // sort on distance
            var sortedOnDistance = creatures.OrderBy(c => MathTools.GetDistance2(c.Place.Position, MyCreature.Place.Position));

            // Filter
            var filtered = new List<IEntity>();
            foreach(var entity in sortedOnDistance)
            {
                // Not me
                if (entity == MyCreature)
                    continue;
                // Not my family
                if (entity.SpawnPoint == MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            _eyeSee.Clear();

            _eyeSee.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(filtered));
            _eyeSee.Add(_leftEye, _leftEye.DistanceToFirstVisible(filtered));
            _eyeSee.Add(_rightEye, _rightEye.DistanceToFirstVisible(filtered));
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
