﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Statistics;

namespace DawnOnline.Simulation.Entities
{
    public interface IEntity
    {
        bool Alive { get; }
        bool IsTired { get; }
        CreatureType Specy { get; }
        Placement Place { get; }
        CharacterSheet CharacterSheet { get; }

        void ApplyActionQueue(double timeDelta);
        void ClearActionQueue();
        void Move();
        bool CanAttack();
    }
}
