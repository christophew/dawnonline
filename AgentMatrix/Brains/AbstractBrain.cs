using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;

namespace DawnOnline.AgentMatrix.Brains
{
    internal abstract class AbstractBrain : IBrain
    {
        public void SetCreature(ICreature creature)
        {
            MyCreature = creature;
            Debug.Assert(MyCreature != null);
        }

        internal ICreature MyCreature { get; private set; }

        public abstract void DoSomething(TimeSpan timeDelta);

        public virtual void InitializeSenses()
        {}

        public virtual void ClearState()
        {}

        private bool _randomTurningLeft;
        private bool _randomTurningRight;
        private bool _randomMoveForward;
        private DateTime _randomMoveStart;

        protected void DoRandomAction(int milliseconds)
        {
            var now = DateTime.Now;

            // keep doing the same thing for a certain amount of milliseconds
            if ((now - _randomMoveStart).TotalMilliseconds < milliseconds)
            {
                if (_randomTurningLeft)
                {
                    MyCreature.WalkForward();
                    MyCreature.TurnLeftSlow();
                }
                if (_randomTurningRight)
                {
                    MyCreature.WalkForward();
                    MyCreature.TurnRightSlow();
                }
                if (_randomMoveForward)
                {
                    MyCreature.WalkForward();
                }
                return;
            }

            _randomTurningLeft = false;
            _randomTurningRight = false;
            _randomMoveForward = false;
            _randomMoveStart = now;


            int randomAction = Globals.Radomizer.Next(5);

            if (randomAction == 0)
            {
                MyCreature.TurnLeft();
                _randomTurningLeft = true;
                return;
            }
            if (randomAction == 1)
            {
                MyCreature.TurnRight();
                _randomTurningRight = true;
                return;
            }

            MyCreature.WalkForward();
            _randomMoveForward = true;
        }

        public virtual IBrain Replicate(IBrain mate)
        {
            throw new NotImplementedException();
        }

        public virtual void Mutate()
        {
        }

        protected List<IEntity> FilterAndSortOnDistance(IEnumerable<IEntity> entities)
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
    }
}
