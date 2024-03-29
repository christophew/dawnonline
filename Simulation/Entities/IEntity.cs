﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Statistics;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    public interface IEntity
    {
        int Id { get; }
        EntityTypeEnum EntityType { get; }
        CreatureTypeEnum CreatureType { get; }
        Placement Place { get; }

        void Update(double timeDelta);
    }
}
