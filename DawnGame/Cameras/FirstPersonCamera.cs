using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DawnGame.Cameras
{
    public class FirstPersonCamera : ICamera
    {
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Position { get { return _position; } }
        public bool FogEnabled { get { return true; } }

        //private Matrix World;

        private float _elevation;
        private float _rotation;
        private Vector3 _position;
        private float _velocity;

        public FirstPersonCamera(GameWindow window, float velocity)
        {
            //World = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                window.ClientBounds.Width / window.ClientBounds.Height, 0.1f, 10000.0f);

            _position = new Vector3(0, 0, 0);
            _rotation = 0f;
            _elevation = 0f;
            _velocity = velocity;
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
                _rotation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad4))
                _rotation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad9))
                _elevation -= MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad7))
                _elevation += MathHelper.Pi * timeScale;
            else if (keys.IsKeyDown(Keys.NumPad2))
            {
                var target = Vector3.Forward * timeScale * _velocity;
                Matrix transform = Matrix.CreateRotationX(_elevation)
                                   *Matrix.CreateRotationY(_rotation);
                Vector3.Transform(ref target, ref transform, out target);
                _position -= target;
            }
            else if (keys.IsKeyDown(Keys.NumPad8))
            {
                var target = Vector3.Forward * timeScale * _velocity;
                Matrix transform = Matrix.CreateRotationX(_elevation)
                                   * Matrix.CreateRotationY(_rotation);
                Vector3.Transform(ref target, ref transform, out target);
                _position += target;
            }

            _rotation = MathHelper.WrapAngle(_rotation);
            _elevation = MathHelper.WrapAngle(_elevation);

            UpdateView();
        }

        private Vector3 CreateTarget()
        {
             var target = Vector3.Forward;
            Matrix transform = Matrix.CreateRotationX(_elevation)
                             * Matrix.CreateRotationY(_rotation)
                             * Matrix.CreateTranslation(_position);
            Vector3.Transform(ref target, ref transform, out target);
            return target;
        }

        private void UpdateView()
        {

            View = Matrix.CreateLookAt(_position, CreateTarget(), Vector3.Up);
        }    
    }
}
