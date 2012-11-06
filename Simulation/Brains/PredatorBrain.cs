using System.Collections.Generic;
using System.Diagnostics;
using System;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Senses;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;
using System.Linq;
using SharedConstants;

namespace DawnOnline.Simulation.Brains
{
    internal class PredatorBrain : AbstractBrain
    {
        protected Eye _forwardEye;
        protected Eye _leftEye;
        protected Eye _rightEye;
        protected Bumper _forwardBumper;
        protected bool _initialized;

        protected Dictionary<Eye, double> _eyeSeeEnemy = new Dictionary<Eye, double>();
        protected Dictionary<Eye, double> _eyeSeeTreasure = new Dictionary<Eye, double>();
        protected Dictionary<Eye, double> _eyeSeeWalls = new Dictionary<Eye, double>();

        protected Ear _leftEar;
        protected Ear _rightEar;

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


        internal override void DoSomething(TimeSpan timeDelta)
        {
            Debug.Assert(MyCreature != null);
            Debug.Assert(_initialized);

            // Fill eye buffer
            SeeEnemies();
            SeeTreasures();
            SeeWalls();

            // Emotional states
            // * neutral
            // * adrenaline
            // * fear
            // * tired

            // AAAAAA... I'm about to die!!
            if (MyCreature.CharacterSheet.Damage.IsCritical &&
                (!_imPanickingSince.HasValue || ((DateTime.Now - _imPanickingSince.Value).Milliseconds > 5000)))
            {
                FearState(timeDelta);
                return;
            }
            if ((DateTime.Now - _lastTimeISawAnEnemy).TotalMilliseconds < 2000 || ISeeAnEnemy())
            {
                if (MyCreature.CharacterSheet.Fatigue.IsCritical)
                {
                    // Shit! I'm tired & more enemies are coming!
                    // RUN AWAY!!
                    FearState(timeDelta);
                    return;
                }

                // CHAAAAARGE!
                AdrenalineState(timeDelta);
                return;
            }
            // REST
            if (MyCreature.CharacterSheet.Fatigue.PercentFilled != 0)
            {
                TiredState(timeDelta);
                return;
            }

            NeutralState(timeDelta);
        }

        protected virtual bool ISeeAnEnemy()
        {
            //var check = _eyeSee[_forwardEye] > 0 || _eyeSee[_leftEye] > 0 || _eyeSee[_rightEye] > 0;
            // get a good look!
            var check = _eyeSeeEnemy[_forwardEye] > 5 || _eyeSeeEnemy[_leftEye] > 5 || _eyeSeeEnemy[_rightEye] > 5;

            if (check)
                _lastTimeISawAnEnemy = DateTime.Now;
            return check;
        }

        protected virtual void NeutralState(TimeSpan timeDelta)
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

        protected virtual void AdrenalineState(TimeSpan timeDelta)
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
            if (_eyeSeeEnemy[_forwardEye] > 0)
            {
                MyCreature.RunForward();
            }
            if (_eyeSeeEnemy[_leftEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_eyeSeeEnemy[_rightEye] > 0)
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

        protected virtual void FearState(TimeSpan timeDelta)
        {
            if (!_imPanickingSince.HasValue)
                _imPanickingSince = DateTime.Now;

            // Run away!!!
            if (_eyeSeeEnemy[_forwardEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }
            if (_eyeSeeEnemy[_leftEye] > 0)
            {
                MyCreature.TurnRight();
                return;
            }
            if (_eyeSeeEnemy[_rightEye] > 0)
            {
                MyCreature.TurnLeft();
                return;
            }

            // All clear!!
            MyCreature.RunForward();
        }

        protected virtual void TiredState(TimeSpan timeDelta)
        {
            // Rest
            MyCreature.RegisterRest();
        }

        private void SeeEnemies()
        {
            var entities = MyCreature.MyEnvironment.GetCreatures(MyCreature.FoodSpecies);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            // Filter
            var filtered = new List<IEntity>();
            foreach(ICreature entity in sortedOnDistance)
            {
                // Not me
                if (entity == MyCreature)
                    continue;
                // Not my family
                if (entity.SpawnPoint == MyCreature.SpawnPoint)
                    continue;

                filtered.Add(entity);
            }

            _eyeSeeEnemy.Clear();

            _eyeSeeEnemy.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_leftEye, _leftEye.DistanceToFirstVisible(filtered));
            _eyeSeeEnemy.Add(_rightEye, _rightEye.DistanceToFirstVisible(filtered));
        }

        private void SeeTreasures()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Treasure);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            _eyeSeeTreasure.Clear();

            _eyeSeeTreasure.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_leftEye, _leftEye.DistanceToFirstVisible(sortedOnDistance));
            _eyeSeeTreasure.Add(_rightEye, _rightEye.DistanceToFirstVisible(sortedOnDistance));
        }

        private List<IEntity> FilterAndSortOnDistance(IEnumerable<IEntity> entities)
        {
            var creaturePosition = MyCreature.Place.Position;

            // exclude all entities outside the bounding box of the vision range
            var minX = creaturePosition.X - MyCreature.CharacterSheet.VisionDistance;
            var maxX = creaturePosition.X + MyCreature.CharacterSheet.VisionDistance;
            var minY = creaturePosition.Y - MyCreature.CharacterSheet.VisionDistance;
            var maxY = creaturePosition.Y + MyCreature.CharacterSheet.VisionDistance;
            var boxOptimizedList = new List<IEntity>();
            foreach (var entity in entities)
            {
                var entityPosition = entity.Place.Position;
                if (entityPosition.X < minX)
                    continue;
                if (entityPosition.X > maxX)
                    continue;
                if (entityPosition.Y < minY)
                    continue;
                if (entityPosition.Y > maxY)
                    continue;

                boxOptimizedList.Add(entity);
            }

            // Filter on exact distance
            var maxDistance2 = MyCreature.CharacterSheet.VisionDistance * MyCreature.CharacterSheet.VisionDistance;
            var filteredWithDistance = new List<KeyValuePair<IEntity, double>>();
            foreach (var entity in boxOptimizedList)
            {
                var distance2 = MathTools.GetDistance2(entity.Place.Position, creaturePosition);
                if (distance2 > maxDistance2)
                    continue;
                filteredWithDistance.Add(new KeyValuePair<IEntity, double>(entity, distance2));
            }

            // Sort on exact distance
            var optimized = filteredWithDistance
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            return optimized;
        }

        private void SeeWalls()
        {
            var entities = MyCreature.MyEnvironment.GetObstacles().Where(e => e.Specy == EntityType.Wall || e.Specy == EntityType.Box);
            var sortedOnDistance = FilterAndSortOnDistance(entities);

            _eyeSeeWalls.Clear();

            _eyeSeeWalls.Add(_forwardEye, _forwardEye.DistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_leftEye, _leftEye.DistanceToFirstVisible(sortedOnDistance, false));
            _eyeSeeWalls.Add(_rightEye, _rightEye.DistanceToFirstVisible(sortedOnDistance, false));
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
            _forwardBumper = new Bumper(MyCreature, new Vector2((float)MyCreature.Place.Form.BoundingCircleRadius, 0));

            // Ears
            _leftEar = new Ear(MyCreature, new Vector2(0, -2));
            _rightEar = new Ear(MyCreature, new Vector2(0, -2));


            _initialized = true;
        }

        internal override void ClearState()
        {
            _forwardBumper.Clear();
        }
    }
}
