using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DawnGame.Cameras
{
    public class AvatarCamera : ICamera
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public string GetDebugString()
        {
            return "TODO";
        }

        public void Update(GameTime gameTime)
        {
            UpdateViewMatrix();
        }

        private Creature _creature;

        internal AvatarCamera(GraphicsDevice device, Creature creature)
        {
            _creature = creature;

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                device.Viewport.AspectRatio,
                1f,
                50000f);

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            var pos = _creature.Place.Position;
            var angle = _creature.Place.Angle;

            var camPosition = new Vector3((float)(pos.X), 20, (float)(pos.Y));
            var cameraLookAt = new Vector3((float)(pos.X + Math.Cos(angle) * 10), 17, (float)(pos.Y + Math.Sin(angle) * 10));

            View = Matrix.CreateLookAt(camPosition, cameraLookAt, Vector3.Up);
        }
    }
}
