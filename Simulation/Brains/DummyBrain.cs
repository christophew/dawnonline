using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOnline.Simulation.Brains
{
    class DummyBrain : IBrain
    {
        #region IBrain Members

        public void DoSomething(TimeSpan timeDelta)
        {
            throw new NotImplementedException();
        }

        public void SetCreature(Entities.ICreature creature) {}
        
        public void InitializeSenses() {}


        public void ClearState()
        {
            throw new NotImplementedException();
        }

        public IBrain Replicate(IBrain mate)
        {
            throw new NotImplementedException();
        }

        public void Mutate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
