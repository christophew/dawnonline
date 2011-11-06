using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Brains
{
    class SpawnPointBrain : AbstractBrain
    {
        private readonly double _maxInterval;
        private readonly EntityType _spawnType;
        private double _interval;
        private DateTime _lastSpawn;

        internal SpawnPointBrain(EntityType spawnType, double interval)
        {
            _spawnType = spawnType;
            _maxInterval = interval;
        }

        internal override void DoSomething()
        {
            if ((DateTime.Now - _lastSpawn).TotalSeconds < _interval)
                return;

            var creature = CreatureBuilder.CreateCreature(_spawnType, this.MyCreature);
            Environment.GetWorld().AddCreature(creature, MyCreature.Place.Position, Globals.Radomizer.NextDouble()*Math.PI*2.0, false);

            _lastSpawn = DateTime.Now;
            _interval = Globals.Radomizer.NextDouble()*_maxInterval;
        }
    }
}
