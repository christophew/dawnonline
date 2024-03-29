﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnClient;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DawnGame.Cameras
{
    public class AvatarCamera : ICamera
    {
        private float _lookatHeight = 1.4f;
        private float _cameraHeight = 1.5f;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Position { get; private set; }
        public bool FogEnabled { get { return true; } }


        public string GetDebugString()
        {
            return string.Format("Height: {0:00.00}", _lookatHeight);
        }

        public void Update(GameTime gameTime)
        {
            const float velocity = 10;

            KeyboardState keys = Keyboard.GetState();
            float timeScale = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keys.IsKeyDown(Keys.NumPad8))
                _lookatHeight += velocity * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad2))
                _lookatHeight -= velocity * timeScale;

            UpdateViewMatrix();
        }

        private DawnClientEntity _creature;

        internal AvatarCamera(GraphicsDevice device, DawnClientEntity creature)
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
            var pos = new Vector2(_creature.PlaceX, _creature.PlaceY);
            var angle = _creature.Angle;

            Position = new Vector3((float)(pos.X), _cameraHeight, (float)(pos.Y));
            var cameraLookAt = new Vector3((float)(pos.X + Math.Cos(angle) * 10), _lookatHeight, (float)(pos.Y + Math.Sin(angle) * 10));

            View = Matrix.CreateLookAt(Position, cameraLookAt, Vector3.Up);
        }
    }
}
