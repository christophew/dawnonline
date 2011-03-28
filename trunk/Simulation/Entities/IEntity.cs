using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Statistics;

namespace DawnOnline.Simulation.Entities
{
    public enum EntityType
    {
        Unknown,
        Avatar,
        Predator,
        Rabbit,
        Plant,
        Turret,
        Wall,
        Box,
        Treasure,
        PredatorFactory,
        Bullet
    }

    public interface IEntity
    {
        EntityType Specy { get; }
        Placement Place { get; }
    }
}
