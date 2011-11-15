using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Statistics;

namespace DawnOnline.Simulation.Entities
{
    public interface ICreature : IEntity
    {
        bool Alive { get; }
        bool IsTired { get; }
        CharacterSheet CharacterSheet { get; }

        void ApplyActionQueue(double timeDelta);
        void ClearActionQueue();
        void Think();
        bool CanAttack();

        IEntity SpawnPoint { get; }

        ICreature Replicate();
        void Mutate();
    }
}
