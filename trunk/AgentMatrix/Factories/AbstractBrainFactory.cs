using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.Simulation.Entities;
using SharedConstants;

namespace DawnOnline.AgentMatrix.Factories
{
    abstract class AbstractBrainFactory
    {
        abstract public AbstractBrain CreateBrainFor(CreatureTypeEnum specy);
        abstract public AbstractBrain CreateSpawnPointBrain(ICreature prototype);
    }
}
