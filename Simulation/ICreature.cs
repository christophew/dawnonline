using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Senses;

namespace DawnOnline.Simulation
{
    public enum CreatureType
    {
        Unknown,
        Avatar,
        Predator,
        Rabbit,
        Plant
    }

    public interface ICreature
    {
        CreatureType Specy { get; }

        bool Alive { get; }

        bool IsTired { get; }

        void Rest();
        void WalkForward(); 
        void RunForward();
        void TurnLeft();
        void TurnRight();

        void Move();

        ICreature Attack();
        bool SeesACreatureForward();
        bool SeesACreatureLeft();
        bool SeesACreatureRight();

        IPlacement Place { get; }
        IMovement Movement { get; }

        IList<IEye> Eyes { get; }
    }
}
