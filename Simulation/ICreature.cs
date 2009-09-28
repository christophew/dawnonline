﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation
{
    public interface ICreature
    {
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
    }
}
