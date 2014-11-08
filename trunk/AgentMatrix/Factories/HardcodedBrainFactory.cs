using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Factories
{
    class HardcodedBrainFactory : AbstractBrainFactory
    {
        public override Brains.AbstractBrain CreateBrainFor(SharedConstants.CreatureTypeEnum specy)
        {
            // TODO: resource gathering behaviour was never implemented inthese brains
            // = ability to take the resources back to the spawnpoint


            if (specy == CreatureTypeEnum.Predator)
            {
                return new PredatorBrain();
            }
            else if (specy == CreatureTypeEnum.Predator2)
            {
                return new PredatorBrain();
            }
            else if (specy == CreatureTypeEnum.Rabbit)
            {
                return new PredatorBrain();
            }

            throw new NotSupportedException();
        }

        public override Brains.AbstractBrain CreateSpawnPointBrain(Simulation.Entities.ICreature prototype)
        {
            return new SpawnPointBrain(prototype);
        }
    }
}
