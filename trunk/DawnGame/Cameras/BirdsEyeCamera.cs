using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DawnGame.Cameras
{
    class BirdsEyeCamera : ICamera
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Position { get { return _cameraPosition; } }
        public bool FogEnabled { get { return false; } }

        private Vector3 _cameraPosition;
        private float _cameraVelocity;
        private float _pan;

        public BirdsEyeCamera(GraphicsDevice device, Vector3 position, float velocity)
        {
            _cameraPosition = position;
            _cameraVelocity = velocity;

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

            // Left/Right
            if (keyboardState.IsKeyDown(Keys.NumPad4))
                _cameraPosition.X -= _cameraVelocity * timeScale;
            if (keyboardState.IsKeyDown(Keys.NumPad6))
                _cameraPosition.X += _cameraVelocity * timeScale;

            // Up/Down
            if (keyboardState.IsKeyDown(Keys.NumPad8))
                _cameraPosition.Z -= _cameraVelocity * timeScale;
            if (keyboardState.IsKeyDown(Keys.NumPad2))
                _cameraPosition.Z += _cameraVelocity * timeScale;

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
