using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DawnGame.Cameras;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace DawnGame
{
    public class DefaultGameBehaviour : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        private Stopwatch _drawTimer = new Stopwatch();
        private Stopwatch _updateTimer = new Stopwatch();
        private double _lastDrawTime = 0;


        private SpriteFont font;


        private DawnWorld _dawnWorld = new DawnWorld();
        private DawnWorldRenderer _dawnWorldRenderer;


        private GameObject _floor;


        private Texture2D _wallTexture;

        private ICamera _camera;

        public DefaultGameBehaviour(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _dawnWorldRenderer = new DawnWorldRenderer(Game, _dawnWorld);
            _dawnWorldRenderer.LoadContent();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Game.Content.Load<SpriteFont>(@"fonts\MyFont");


            _wallTexture = Game.Content.Load<Texture2D>(@"Textures\brickThumb");

            _floor = new GameObject(Game.Content.Load<Model>(@"floor_metal"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -2025, 0), 2000f);

            _camera = new BirdsEyeFollowCamera(GraphicsDevice, 80, 500, _dawnWorld.Avatar);
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
        public override void Update(GameTime gameTime)
        {
            _updateTimer.Reset();
            _updateTimer.Start();

            


            _dawnWorldRenderer.Update(gameTime);


            SwitchCamera();
            _camera.Update(gameTime);


            base.Update(gameTime);


            _updateTimer.Stop();
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

        /// <summary>
        /// This is called when the object should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            _drawTimer.Reset();
            _drawTimer.Start();


            //GraphicsDevice.Clear(Color.WhiteSmoke);
            GraphicsDevice.Clear(Color.Black);

            _dawnWorldRenderer.Draw(gameTime, _camera);

            _floor.DrawObject(_camera, new Vector3(_dawnWorld.Center.X, 0, _dawnWorld.Center.Y), Vector3.Zero);


            DrawTextInfo();

            base.Draw(gameTime);

            _drawTimer.Stop();
            _lastDrawTime = _drawTimer.ElapsedMilliseconds;
        }

        private void DrawTextInfo()
        {
            spriteBatch.Begin();

            var worldInformation = _dawnWorld.GetWorldInformation();
            spriteBatch.DrawString(font, worldInformation, new Vector2(100f, 100f), Color.Green);

            string technicalInformation = string.Format("Think: {0:0000}ms; Move: {1:0000}ms; Update: {2:0000}ms; Draw: {3:0000}ms",
                                                        _dawnWorldRenderer.ThinkTime, _dawnWorldRenderer.MoveTime, _updateTimer.ElapsedMilliseconds, _lastDrawTime);
            spriteBatch.DrawString(font, technicalInformation, new Vector2(100f, 150f), Color.Green);

            if (_dawnWorld.Avatar != null)
            {
                string stats = string.Format("Damage: {0}%; Velocity: {1:000.0}", 
                    _dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled,
                    _dawnWorld.Avatar.Place.Velocity);
                spriteBatch.DrawString(font, stats, new Vector2(100f, 200f), Color.Green);
            }

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
