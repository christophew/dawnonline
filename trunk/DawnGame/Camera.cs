using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DawnGame
{
    public class RotatingCamera
    {

        public Matrix Projection;
        public Matrix View;
        public Matrix World;

        float distance;
        float rotation;
        float elevation;

        public RotatingCamera()
        {

            World = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(
               MathHelper.PiOver2, 1f, 0.1f, 1000f);

            distance = 4f;
            rotation = 0f;
            elevation = 0f;
        }

        public void Update(GameTime gameTime)
        {

            KeyboardState keys = Keyboard.GetState();
            float timeScale = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keys.IsKeyDown(Keys.Right))
                rotation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.Left))
                rotation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.PageUp))
                elevation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.PageDown))
                elevation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.Up))
                distance -= 2f * timeScale;
            else if (keys.IsKeyDown(Keys.Down))
                distance += 2f * timeScale;

            rotation = MathHelper.WrapAngle(rotation);
            elevation = MathHelper.WrapAngle(elevation);
            distance = MathHelper.Clamp(distance, 0f, 15f);

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
