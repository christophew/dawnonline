using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DawnGame.Cameras
{
    class BirdsEyeFollowCamera : ICamera
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        private Vector3 _cameraPosition;
        private float _cameraVelocity;
        private float _pan;
        private ICreature _creature;

        public BirdsEyeFollowCamera(GraphicsDevice device, float height, float velocity, ICreature creature)
        {
            _cameraPosition = new Vector3(creature.Place.Position.X, height, creature.Place.Position.Y);
            _cameraVelocity = velocity;
            _creature = creature;

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                device.Viewport.AspectRatio,
                1f,
                50000f);

            UpdateViewMatrix();
        }

        public string GetDebugString()
        {
            return string.Format("Camera position: ({0}, {1}, {2}); pan: {3}", (int) _cameraPosition.X, (int) _cameraPosition.Y, (int) _cameraPosition.Z, _pan);
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            float timeScale = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _cameraPosition.X = _creature.Place.Position.X;
            _cameraPosition.Z = _creature.Place.Position.Y;

            // In/Out
            if (keyboardState.IsKeyDown(Keys.NumPad7))
                _cameraPosition.Y += _cameraVelocity * timeScale;
            if (keyboardState.IsKeyDown(Keys.NumPad9))
                _cameraPosition.Y -= _cameraVelocity * timeScale;

            // Pan
            if (keyboardState.IsKeyDown(Keys.NumPad1))
                _pan += _cameraVelocity * timeScale;
            if (keyboardState.IsKeyDown(Keys.NumPad3))
                _pan -= _cameraVelocity * timeScale;

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Vector3 cameraLookAt = new Vector3(_cameraPosition.X, 0f, _cameraPosition.Z + _pan);

            //Vector3 cameraLookAt = Vector3.Zero;

            View = Matrix.CreateLookAt(_cameraPosition, cameraLookAt, Vector3.Forward);
        }

    }
}
