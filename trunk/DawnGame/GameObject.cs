using DawnGame.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DawnGame
{
    class GameObject
    {
        private Model _model = null;
        private Vector3 _originalPosition = Vector3.Zero;
        private Vector3 _originalRotation = Vector3.Zero;
        private float _scale = 1f;

        public GameObject(Model model, Vector3 rotation, Vector3 position, float scale)
        {
            _model = model;
            _originalRotation = rotation;
            _originalPosition = position;
            _scale = scale;
        }

        public void DrawObject(ICamera camera, Vector3 position, Vector3 rotation)
        {
            var totalRotation = _originalRotation + rotation;
            var totalPosition = _originalPosition + position;
            var worldMatrix = Matrix.CreateFromYawPitchRoll(totalRotation.Y, totalRotation.X, totalRotation.Z)
                              *Matrix.CreateScale(_scale)
                              *Matrix.CreateTranslation(totalPosition);

            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = worldMatrix;
                    effect.Projection = camera.Projection;
                    effect.View = camera.View;


                    // testen
                    effect.FogEnabled = camera.FogEnabled;
                    effect.FogColor = Color.Gray.ToVector3();
                    //effect.FogStart = 9.75f;
                    //effect.FogEnd = 10.25f;
                    effect.FogStart = 200;
                    effect.FogEnd = 2000;

                }
                mesh.Draw();
            }
        }
    }
}
