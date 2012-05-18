using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation
{
    internal class Sound
    {
        public enum SoundTypeEnum
        {
            A, B
        }

        public Vector2 Position { get; private set; }
        public SoundTypeEnum SoundType { get; private set; }
        public int SoundFamily { get; private set; }
        public double Volume { get; private set; }
        public long DurationInMs { get; private set; }

        internal Sound(Vector2 position, SoundTypeEnum soundType, int soundFamily, double volume, long durationInMs)
        {
            Position = position;
            SoundType = soundType;
            SoundFamily = soundFamily;
            Volume = volume;
            DurationInMs = durationInMs;
        }

        internal bool Update(long timeDelta)
        {
            DurationInMs -= timeDelta;
            return DurationInMs > 0;
        }
    }
}
