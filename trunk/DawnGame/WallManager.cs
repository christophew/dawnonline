using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TexturedBox;

namespace DawnGame
{
    class WallManager
    {
        private BasicEffect _wallEffect;
        private Texture2D _wallTexture;

        public WallManager(GraphicsDevice device, Texture2D wallTexture)
        {
            _wallEffect = new BasicEffect(device);
            _wallEffect.Texture = wallTexture;
            _wallEffect.TextureEnabled = true;
        }

        public void Draw(List<BasicShape> walls, GraphicsDevice device, Matrix cameraMatrix, Matrix projectionMatrix)
        {
            foreach (var wall in walls)
            {
                var pass = _wallEffect.CurrentTechnique.Passes[0];

                _wallEffect.World = wall.worldMatrix;
                _wallEffect.View = cameraMatrix;
                _wallEffect.Projection = projectionMatrix;

                pass.Apply();

                wall.RenderShape(device);
            }
        }

    }
}
