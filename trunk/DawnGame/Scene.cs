using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnGame.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredLighting
{
    class Scene
    {
        private Game game;
        Model[] models;
        public Scene(Game game)
        {
            this.game = game;
        }
        public void InitializeScene()
        {
            models = new Model[4];
            models[0] = game.Content.Load<Model>(@"DeferredLighting\ship1");
            models[1] = game.Content.Load<Model>(@"DeferredLighting\ship2");
            models[2] = game.Content.Load<Model>(@"DeferredLighting\lizard");
            models[3] = game.Content.Load<Model>(@"DeferredLighting\ground");
        }
        public void DrawScene(ICamera camera, GameTime gameTime)
        {
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            
            DrawModel(models[0], Matrix.CreateTranslation(-30, 0, -20), camera);
            DrawModel(models[1], Matrix.CreateTranslation(30, 0, -20), camera);
            DrawModel(models[2], Matrix.CreateScale(0.05f) * Matrix.CreateTranslation(0, 0, 27), camera);
            DrawModel(models[3], Matrix.CreateTranslation(0, -10, 0), camera);
        }
        private void DrawModel(Model model, Matrix world, ICamera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                }
                mesh.Draw();
            }
        }
    }
}
