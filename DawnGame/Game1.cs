using System;
using System.Collections.Generic;
using DawnGame.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using RoundLineCode;
using System.Diagnostics;
using TexturedBox;

namespace DawnGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Stopwatch _drawTimer = new Stopwatch();
        private Stopwatch _updateTimer = new Stopwatch();
        private double _lastDrawTime = 0;


        private SpriteFont font;


        private DawnClient.DawnClient _dawnClient = new DawnClient.DawnClient();
        private DawnWorldRenderer _dawnWorldRenderer;


        private GameObject _floor;


        private Texture2D _wallTexture;

        private ICamera _camera;


        Viewport defaultViewport;
        Viewport leftViewport;
        Viewport rightViewport;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 3000;
            graphics.PreferredBackBufferHeight = 2000;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            System.Windows.Forms.Control form = System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.Location = new System.Drawing.Point(0, 0);

            _dawnClient.Connect();
            _dawnWorldRenderer = new DawnWorldRenderer(this, _dawnClient);

            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 100);
            IsFixedTimeStep = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _dawnWorldRenderer.LoadContent();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>(@"fonts\MyFont");


            _wallTexture = Content.Load<Texture2D>(@"Textures\brickThumb");

            _floor = new GameObject(Content.Load<Model>(@"floor_metal"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -2025, 0), 2000f);

            //_camera = new BirdsEyeFollowCamera(GraphicsDevice, 80, 50, _dawnWorld.Avatar);
            _camera = new FirstPersonCamera(Window, 10);

            // Viewports
            defaultViewport = GraphicsDevice.Viewport;
            leftViewport = defaultViewport;
            rightViewport = defaultViewport;
            leftViewport.Width = leftViewport.Width / 2;
            rightViewport.Width = rightViewport.Width / 2;
            rightViewport.X = leftViewport.Width;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _updateTimer.Reset();
            _updateTimer.Start();

            
            KeyboardState keyboardState = Keyboard.GetState();

            // Exit
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }


            _dawnWorldRenderer.Update(gameTime);


            SwitchCamera();
            _camera.Update(gameTime);


            base.Update(gameTime);


            _updateTimer.Stop();
        }


        private void SwitchCamera()
        {
            //var keyboard = Keyboard.GetState();

            //if (keyboard.IsKeyDown(Keys.F1))
            //    _camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(DawnWorld.MaxX / 2f, 430, DawnWorld.MaxY / 2f), 100);
            //if (keyboard.IsKeyDown(Keys.F2))
            //    _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            //if (keyboard.IsKeyDown(Keys.F3))
            //    _camera = new BirdsEyeFollowCamera(GraphicsDevice, 100, 50, _dawnWorld.Avatar);
            //if (keyboard.IsKeyDown(Keys.F4))
            //    _camera = new FirstPersonCamera(Window, 10);
            //if (keyboard.IsKeyDown(Keys.F5))
            //    _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Environment.GetCreatures(EntityType.Predator)[0]);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _drawTimer.Reset();
            _drawTimer.Start();

            // Viewports
            GraphicsDevice.Viewport = defaultViewport;
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Viewport = leftViewport;
            //_camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            DrawScene(gameTime);

            GraphicsDevice.Viewport = rightViewport;
            //_camera = new BirdsEyeFollowCamera(GraphicsDevice, 100, 50, _dawnWorld.Avatar);
            DrawScene(gameTime);

            base.Draw(gameTime);

            _drawTimer.Stop();
            _lastDrawTime = _drawTimer.ElapsedMilliseconds;
        }

        private void DrawScene(GameTime gameTime)
        {
            _dawnWorldRenderer.Draw(gameTime, _camera);

            //DrawSkyDome();
            //_floor.DrawObject(_camera, new Vector3(_dawnWorld.Center.X, 0, _dawnWorld.Center.Y), Vector3.Zero);

            DrawTextInfo();
        }

        private void DrawTextInfo()
        {
            spriteBatch.Begin();

            var worldInformation = _dawnClient.DawnWorld.WorldInformation;
            spriteBatch.DrawString(font, worldInformation, new Vector2(100f, 100f), Color.Green);

            string technicalInformation = string.Format("Think: {0:0000}ms; Move: {1:0000}ms; Update: {2:0000}ms; Draw: {3:0000}ms",
                                                        _dawnWorldRenderer.ThinkTime, _dawnWorldRenderer.MoveTime, _updateTimer.ElapsedMilliseconds, _lastDrawTime);
            spriteBatch.DrawString(font, technicalInformation, new Vector2(100f, 150f), Color.Green);

            //if (_dawnWorld.Avatar != null)
            //{
            //    string stats = string.Format("Damage: {0}%; Velocity: {1:000.0}", 
            //        _dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled,
            //        _dawnWorld.Avatar.Place.Velocity);
            //    spriteBatch.DrawString(font, stats, new Vector2(100f, 200f), Color.Green);
            //}

            spriteBatch.DrawString(font, _camera.GetDebugString(), new Vector2(100f, 250f), Color.Green);

            spriteBatch.End();


            // Fix Spritebatch problems
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            }
        }

    }
}
