using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Entities;
using DawnOnline.Simulation.Tools;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Senses
{
    internal class Ear
    {
        private readonly Creature _creature;
        private readonly Vector2 _relativePosition;

        internal Ear(Creature creature, Vector2 relativePosition)
        {
            _creature = creature;
            _relativePosition = relativePosition;
        }

        internal double HearFamily(Sound.SoundTypeEnum soundType)
        {
            var sounds = _creature.MyEnvironment.GetSounds(soundType);
            var familySounds = sounds.Where(sound => sound.SoundFamily == _creature.FamilyId);

            return SummateSounds(familySounds);
        }

        internal double HearStrangers(Sound.SoundTypeEnum soundType)
        {
            var sounds = _creature.MyEnvironment.GetSounds(soundType);
            var strangerSounds = sounds.Where(sound => sound.SoundFamily != _creature.FamilyId);

            return SummateSounds(strangerSounds);
        }

        private double SummateSounds(IEnumerable<Sound> sounds)
        {
            Vector2 earPosition = _creature.Place.Position + _relativePosition;

            double total = 0;
            foreach (var sound in sounds)
            {
                var distance = MathTools.GetDistance(earPosition, sound.Position);
                total += distance < 1 ? sound.Volume : sound.Volume/distance;
            }

            return total;
        }
    }
}
