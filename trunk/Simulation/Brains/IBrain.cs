using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    public interface IBrain
    {
        void DoSomething(TimeSpan timeDelta);

        void SetCreature(ICreature creature);
        void InitializeSenses();
        void ClearState();

        IBrain Replicate(IBrain mate);
        void Mutate();
    }
}
