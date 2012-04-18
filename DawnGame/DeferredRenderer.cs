using System;
using System.Collections.Generic;
using System.Diagnostics;
using DawnGame.Cameras;
using DawnOnline.Simulation.Entities;
using DeferredLighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DawnGame
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class DeferredRenderer : DrawableGameComponent
    {
        private ICamera _camera;
        private DawnWorld _dawnWorld = new DawnWorld();
        private DawnWorldRenderer _dawnWorldRenderer;
        private GameObject _floor;

        private Scene _scene;
        //private Camera camera;

        private QuadRenderComponent quadRenderer;

        private RenderTarget2D colorRT; //color and specular intensity
        private RenderTarget2D normalRT; //normals + specular power
        private RenderTarget2D depthRT; //depth
        private RenderTarget2D lightRT; //lighting

        private Effect clearBufferEffect;
        private Effect directionalLightEffect;

        private Effect pointLightEffect;
        private Model sphereModel; //point ligt volume

        private Effect finalCombineEffect;

        private Vector2 halfPixel;


        public DeferredRenderer(Game game)
            : base(game)
        {
            _scene = new Scene(Game);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            //camera = new Camera(Game);
            //camera.CameraArc = -30;
            //camera.CameraDistance = 50;
            quadRenderer = new QuadRenderComponent(Game);
            //Game.Components.Add(camera);
            Game.Components.Add(quadRenderer);
        }

        protected override void LoadContent()
        {
            halfPixel = new Vector2()
            {
                X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight
            };

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            colorRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None); 
            depthRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            // Scene
            //_scene.InitializeScene();         

            clearBufferEffect = Game.Content.Load<Effect>(@"DeferredLighting\ClearGBuffer");
            directionalLightEffect = Game.Content.Load<Effect>(@"DeferredLighting\DirectionalLight");
            finalCombineEffect = Game.Content.Load<Effect>(@"DeferredLighting\CombineFinal");
            pointLightEffect = Game.Content.Load<Effect>(@"DeferredLighting\PointLight");
            sphereModel = Game.Content.Load<Model>(@"DeferredLighting\sphere");
           
            // Dawn
            _dawnWorldRenderer = new DawnWorldRenderer(Game, _dawnWorld);
            _dawnWorldRenderer.LoadContent();
            _camera = new BirdsEyeFollowCamera(GraphicsDevice, 80, 50, _dawnWorld.Avatar);
            _floor = new GameObject(Game.Content.Load<Model>(@"floor_metal"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -2020, 0), 2000f);
            //_floor = new GameObject(Game.Content.Load<Model>(@"floor4"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -2025, 0), 2000f);

            // Text info
            //font = Game.Content.Load<SpriteFont>(@"fonts\MyFont");



            base.LoadContent();
        }

        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        private void ResolveGBuffer()
        {
            GraphicsDevice.SetRenderTargets(null);            
        }

        private void ClearGBuffer()
        {            
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);            
        }

        private void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);

            directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());

            directionalLightEffect.Parameters["cameraPosition"].SetValue(_camera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_camera.View * _camera.Projection));

            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            directionalLightEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);            
        }
      
        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {            
            //set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(_camera.View);
            pointLightEffect.Parameters["Projection"].SetValue(_camera.Projection);
            //light position
            pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(_camera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(_camera.View * _camera.Projection));
            //size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(_camera.Position, lightPosition);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;                
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.Indices = meshPart.IndexBuffer;
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }            
            
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        
        public override void Draw(GameTime gameTime)
        {
            DrawScene(gameTime);
        }

        private void DrawScene(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.Opaque;
            _dawnWorldRenderer.Draw(gameTime, _camera);

            _floor.DrawObject(_camera, new Vector3(_dawnWorld.Center.X, 0, _dawnWorld.Center.Y), Vector3.Zero);

            //_scene.DrawScene(_camera, gameTime);


            ResolveGBuffer();
            DrawLights(gameTime);
        }

        private void DrawAvatarPointLight(Color color, float height, float radius, float intensity)
        {
            DrawPointLight(
                new Vector3(_dawnWorld.Avatar.Place.Position.X, height, _dawnWorld.Avatar.Place.Position.Y),
                color,
                radius * (1f - (float)_dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled / 100f),
                intensity);
        }

        private void DrawObstaclePointLight(EntityType entityType, Color color, float height, float radius, float intensity)
        {
            var obstacles = _dawnWorld.Environment.GetObstacles();
            foreach (var current in obstacles)
            {
                if (current.Specy == entityType)
                {
                    DrawPointLight(
                        new Vector3(current.Place.Position.X, height, current.Place.Position.Y),
                        color,
                        radius,
                        intensity);
                }
            }
        }

        private void DrawCreaturePointLight(EntityType entityType, Color color, float height, float radius, float intensity)
        {
            var obstacles = _dawnWorld.Environment.GetCreatures();
            foreach (var current in obstacles)
            {
                if (current.Specy == entityType)
                {
                    DrawPointLight(
                        new Vector3(current.Place.Position.X, height, current.Place.Position.Y),
                        color,
                        radius,
                        intensity);
                }
            }
        }

        private static readonly Random _randomize = new Random((int)DateTime.Now.Ticks);
        Dictionary<IEntity, Color> _familyColorMapper = new Dictionary<IEntity, Color>();

        private void DrawFamilyPointLight(EntityType entityType, float height, float radius, float intensity)
        {
            var obstacles = _dawnWorld.Environment.GetCreatures();
            foreach (var current in obstacles)
            {
                if (current.Specy == entityType)
                {
                    Color color;
                    if (!_familyColorMapper.TryGetValue(current.SpawnPoint, out color))
                    {
                        var skipColor = _randomize.Next(3);
                        color = new Color(
                            skipColor == 0 ? 0 : _randomize.Next(255),
                            skipColor == 1 ? 0 : _randomize.Next(255),
                            skipColor == 2 ? 0 : _randomize.Next(255));
                        _familyColorMapper.Add(current.SpawnPoint, color);
                    }

                    var creature = current as ICreature;
                    Debug.Assert(creature != null);

                    DrawPointLight(
                        new Vector3(current.Place.Position.X, height, current.Place.Position.Y),
                        color,
                        radius * (1f - (float)creature.CharacterSheet.Damage.PercentFilled / 100f),
                        intensity);
                }
            }
        }

        private void DrawBulletPointLight(EntityType entityType, Color color, float height, float radius, float intensity)
        {
            var obstacles = _dawnWorld.Environment.GetBullets();
            foreach (var current in obstacles)
            {
                if (current.Specy == entityType)
                {
                    DrawPointLight(
                        new Vector3(current.Place.Position.X, height, current.Place.Position.Y),
                        color,
                        radius,
                        intensity);
                }
            }
        }

        private void DrawExplosions()
        {
            var explosions = _dawnWorld.Environment.GetExplosions();
            foreach (var current in explosions)
            {
                DrawPointLight(
                    new Vector3(current.Position.X, 10, current.Position.Y),
                    Color.Tomato,
                    current.Size,
                    10);
            }
        }

        private void DrawLights(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(lightRT);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            DrawDirectionalLight(new Vector3(-1, -1, 0), Color.Gray);

            MovingPointLights(gameTime, 0);

            DrawAvatarPointLight(Color.White, -1f, 50.0f, 1);
            DrawObstaclePointLight(EntityType.Treasure, Color.Red, .7f, 10.0f, 2);
            DrawObstaclePointLight(EntityType.PredatorFactory, Color.Blue, 10.0f, 50.0f, 1);
            //DrawCreaturePointLight(EntityType.Turret, Color.LightGreen, 7.5f, 15.0f, 2);
            //DrawCreaturePointLight(EntityType.Predator, Color.BlueViolet, 1.0f, 7.5f, 2);
            DrawBulletPointLight(EntityType.Bullet, Color.Firebrick, .3f, 1.5f, 20f);
            DrawBulletPointLight(EntityType.Rocket, Color.Firebrick, .5f, 2.5f, 10f);
            DrawExplosions();

            DrawFamilyPointLight(EntityType.Predator, 3f, 10f, 2);
            DrawFamilyPointLight(EntityType.SpawnPoint, 3f, 20f, 2f);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;            
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetRenderTarget(null);
            
            //Combine everything
            finalCombineEffect.Parameters["colorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["halfPixel"].SetValue(halfPixel);

            finalCombineEffect.Techniques[0].Passes[0].Apply();            
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
            //quadRenderer.Render(Vector2.One * -1, new Vector2(0, 1f));
            //quadRenderer.Render(new Vector2(0, -1f), Vector2.One);
        }

        private void MovingPointLights(GameTime gameTime, int nrOfLight)
        {
            Color[] colors = new Color[10];
            //colors[0] = Color.Red; 
            colors[0] = Color.Gold; 
            colors[1] = Color.Blue;
            colors[2] = Color.IndianRed; colors[3] = Color.CornflowerBlue;
            colors[4] = Color.Gold; colors[5] = Color.Green;
            colors[6] = Color.Crimson; colors[7] = Color.SkyBlue;
            colors[8] = Color.Red; colors[9] = Color.ForestGreen;
            float angle = (float)gameTime.TotalGameTime.TotalSeconds;

            for (int i = 0; i < nrOfLight; i++)
            {
                var x = (float)Math.Sin(i * MathHelper.TwoPi / nrOfLight + angle) * 500 + 1000;
                var y = (float)Math.Cos(i * MathHelper.TwoPi / nrOfLight + angle) * 500 + 1000;

                Vector3 pos = new Vector3(x, 75, y);
                DrawPointLight(pos, colors[i % 10], 1000, 1);
                pos.Y = 50;
                DrawPointLight(pos, colors[i % 10], 1000, 1);
                pos.Y = 25;
                DrawPointLight(pos, colors[i % 10], 1000, 1);

                //Vector3 pos = new Vector3((float)Math.Sin(i * MathHelper.TwoPi / n + angle), 0.30f, (float)Math.Cos(i * MathHelper.TwoPi / n + angle));
                //DrawPointLight(pos * 40, colors[i % 10], 15, 2);
                //pos = new Vector3((float)Math.Cos((i + 5) * MathHelper.TwoPi / n - angle), 0.30f, (float)Math.Sin((i + 5) * MathHelper.TwoPi / n - angle));
                //DrawPointLight(pos * 20, colors[i % 10], 20, 1);
                //pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), 0.10f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                //DrawPointLight(pos * 75, colors[i % 10], 45, 2);
                //pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), -0.3f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                //DrawPointLight(pos * 20, colors[i % 10], 20, 2);
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            _dawnWorldRenderer.Update(gameTime);


            SwitchCamera();
            _camera.Update(gameTime);
         
            base.Update(gameTime);
        }

        private void SwitchCamera()
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.F1))
                _camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(DawnWorld.MaxX / 2f, 430, DawnWorld.MaxY / 2f), 100);
            if (keyboard.IsKeyDown(Keys.F2))
                _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F3))
                _camera = new BirdsEyeFollowCamera(GraphicsDevice, 100, 50, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F4))
                _camera = new FirstPersonCamera(Game.Window, 10);
            if (keyboard.IsKeyDown(Keys.F5))
                _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Environment.GetCreatures(EntityType.Predator)[0]);
        }
    }
}
