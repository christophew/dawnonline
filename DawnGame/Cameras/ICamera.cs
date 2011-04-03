using Microsoft.Xna.Framework;

namespace DawnGame.Cameras
{
    interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
        Vector3 Position { get; }
        bool FogEnabled { get; }

        void Update(GameTime gameTime);

        string GetDebugString();
    }
}
