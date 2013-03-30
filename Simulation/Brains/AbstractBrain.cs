using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    internal abstract class AbstractBrain : IBrain
    {
        public void SetCreature(ICreature creature)
        {
            MyCreature = creature as Creature;
            Debug.Assert(MyCreature != null);
        }

        internal Creature MyCreature { get; private set; }

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
    }
}
