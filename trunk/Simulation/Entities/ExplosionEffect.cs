using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Entities
{
    class ExplosionEffect : IExplosion
    {
        public float Size { get; private set; }
        public Vector2 Position { get; private set; }


        private DateTime _creationTime;
        private int _duration;

        internal ExplosionEffect(Vector2 position, float size, int duration)
        {
            _creationTime = DateTime.Now;
            _duration = duration;
            Size = size;
            Position = position;
        }

        internal bool IsExpired()
        {
            return (DateTime.Now - _creationTime).TotalMilliseconds > _duration;
        }
    }
}
