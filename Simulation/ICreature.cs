using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation
{
    public interface ICreature
    {
        bool Alive { get; }

        void WalkForward();
        void TurnLeft();
        void TurnRight();

        void Move();

        ICreature Attack();
        bool SeesACreatureForward();
        bool SeesACreatureLeft();
        bool SeesACreatureRight();


        IPlacement Place { get; }
    }
}
