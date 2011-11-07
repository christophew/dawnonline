using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    internal abstract class AbstractBrain : ICloneable
    {
        internal Creature MyCreature { get; set; }

        internal abstract void DoSomething();

        internal virtual void InitializeSenses()
        {}

        internal virtual void ClearState()
        {}

        private bool _randomTurningLeft;
        private bool _randomTurningRight;
        private bool _randomMoveForward;
        private DateTime _randomMoveStart;

        protected void DoRandomAction()
        {
            var now = DateTime.Now;

            // keep doing the same thing for a certain amount of milliseconds
            if ((now - _randomMoveStart).TotalMilliseconds < 100)
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

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

        internal virtual AbstractBrain Replicate()
        {
            throw new NotImplementedException();
        }
    }
}
