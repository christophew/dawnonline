using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DawnGame
{
    class SimpleBox
    {
        public Texture2D Texture { get; private set; }
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }

        public SimpleBox(GraphicsDevice gfxDevice, int width, int height)
        {
            Texture = new Texture2D(gfxDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Color);// create the rectangle texture, ,but it will have no color! lets fix that
            Color[] color = new Color[width * height];//set the color to the amount of pixels
            for (int i = 0; i < color.Length; i++)//loop through all the colors setting them to whatever values we want
            {
                color[i] = new Color(0, 0, 0, 255);
            }
            Texture.SetData(color);//set the color data on the texture

            CenterX = width/2;
            CenterY = height/2;
        }
    }
}
