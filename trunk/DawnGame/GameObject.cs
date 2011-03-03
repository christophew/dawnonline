using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DawnGame
{
    class GameObject
    {
        public Model model = null;
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public float scale = 1f;
        public Vector3 velocity = Vector3.Zero;
        public bool alive = false;

        public void DrawObject(Matrix viewMatrix, Matrix projMatrix)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    //effect.World = Matrix.CreateTranslation(gameObject.position);

                    effect.World = Matrix.CreateFromYawPitchRoll(
                                       rotation.Y,
                                       rotation.X,
                                       rotation.Z) *
                                   Matrix.CreateScale(scale) *
                                   Matrix.CreateTranslation(position);


                    effect.Projection = projMatrix;
                    effect.View = viewMatrix;
                }
                mesh.Draw();
            }
        }

    }
}
