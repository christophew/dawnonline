using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DawnGame.Cameras
{
    public class RotatingCamera : ICamera
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        private Matrix World;

        private float distance;
        private float rotation;
        private float elevation;

        public RotatingCamera()
        {
            World = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 0.1f, 1000f);

            distance = 4f;
            rotation = 0f;
            elevation = 0f;
        }

        public string GetDebugString()
        {
            return "TODO";
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            float timeScale = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keys.IsKeyDown(Keys.NumPad6))
                rotation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad4))
                rotation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad9))
                elevation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad7))
                elevation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad8))
                distance -= 2f * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad2))
                distance += 2f * timeScale;

            rotation = MathHelper.WrapAngle(rotation);
            elevation = MathHelper.WrapAngle(elevation);
            distance = MathHelper.Clamp(distance, 0f, 100f);

            UpdateView();
        }

        private void UpdateView()
        {
            Vector3 pos = new Vector3(0f, 0f, distance);
            Matrix transform = Matrix.CreateRotationX(elevation)
                             * Matrix.CreateRotationY(rotation);
            Vector3.Transform(ref pos, ref transform, out pos);

            View = Matrix.CreateLookAt(pos, Vector3.Zero, Vector3.Up);
        }
    }
}
