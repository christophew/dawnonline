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
            DrawObject(camera, position, rotation, false);
        }

        public void DrawObject(ICamera camera, Vector3 position, Vector3 rotation, bool emit)
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
                    //effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = worldMatrix;
                    effect.Projection = camera.Projection;
                    effect.View = camera.View;


                    // testen
                    //effect.FogEnabled = camera.FogEnabled;
                    effect.FogColor = Color.Gray.ToVector3();
                    //effect.FogStart = 9.75f;
                    //effect.FogEnd = 10.25f;
                    effect.FogStart = 100;
                    effect.FogEnd = 1000;

                    effect.LightingEnabled = true;
                    //effect.Alpha = 0.5f;
                    //effect.AmbientLightColor = Color.Black.ToVector3();
                    //effect.DiffuseColor = Color.Black.ToVector3();
                    if (emit)
                    {
                        effect.EmissiveColor = Color.Yellow.ToVector3();
                    }
                    effect.SpecularPower = 10f;
                    //effect.SpecularColor = Color.Green.ToVector3();
                    //effect.PreferPerPixelLighting = true;

                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.Direction = new Vector3(1, 0, 0);
                    //effect.DirectionalLight0.Direction = new Vector3(0.1f, rotation.Y, 0);
                    //effect.DirectionalLight0.DiffuseColor = Color.DarkRed.ToVector3();
                    effect.DirectionalLight0.SpecularColor = Color.Beige.ToVector3();
                }
                mesh.Draw();
            }
        }
    }
}
