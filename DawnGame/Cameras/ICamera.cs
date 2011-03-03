using Microsoft.Xna.Framework;

namespace DawnGame.Cameras
{
    interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }

        void Update(GameTime gameTime);

        string GetDebugString();
    }
}
