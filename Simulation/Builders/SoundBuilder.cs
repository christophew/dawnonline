using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;

namespace DawnOnline.Simulation.Builders
{
    class SoundBuilder
    {
        internal static Sound CreateSoundForCreature(Creature creature, Sound.SoundTypeEnum soundType, double volume)
        {
            Debug.Assert(volume > 0);

            var family = creature.SpawnPoint ?? creature;

            var sound = new Sound(creature.Place.Position, soundType, family.Id, volume, 1);
            return sound;
        }
    }
}
