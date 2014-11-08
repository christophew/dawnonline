using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.AgentMatrix.Brains.Neural;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Factories
{
    class NeuralBrainFactory : AbstractBrainFactory
    {
        public override Brains.AbstractBrain CreateBrainFor(SharedConstants.CreatureTypeEnum specy)
        {
            if (specy != CreatureTypeEnum.Predator &&
                specy != CreatureTypeEnum.Predator2 &&
                specy != CreatureTypeEnum.Rabbit)
                throw new NotImplementedException();


            var brain = new NeuralBrain();
            brain.PredefineBehaviour();
            return brain;
        }

        public override Brains.AbstractBrain CreateSpawnPointBrain(ICreature prototype)
        {
            return new SpawnPointBrain(prototype);
        }

    }
}
