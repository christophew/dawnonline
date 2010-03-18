using System;
using System.Collections.Generic;
using A.Namespace.Of.Your.Choice.Graphics;
using BoxTutorial;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Collision;
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

namespace DawnGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private string _worldInformation = "";
        private Stopwatch _thinkTimer = new Stopwatch();
        private Stopwatch _moveTimer = new Stopwatch();
        private SpriteFont font;
        Matrix projMatrix;
        Matrix viewMatrix;
        Matrix viewMatrix2;

        RoundLineManager roundLineManager;
        float cameraX = 1500;
        float cameraY = 1000;
        float cameraZoom = 1000;
        bool aButtonDown = false;
        int roundLineTechniqueIndex = 0;
        string[] roundLineTechniqueNames;

        DawnWorld _dawnWorld = new DawnWorld();

        Random _randomize = new Random();

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

            //var myViewport = GraphicsDevice.Viewport;
            //myViewport.X = 100;
            //myViewport.Y = 100;
            //myViewport.Height = 100;
            //GraphicsDevice.Viewport = myViewport;
            //graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// Create a simple 2D projection matrix
        /// </summary>
        public void Create2DProjectionMatrix()
        {
            // Projection matrix ignores Z and just squishes X or Y to balance the upcoming viewport stretch
            float projScaleX;
            float projScaleY;
            float width = graphics.GraphicsDevice.Viewport.Width;
            float height = graphics.GraphicsDevice.Viewport.Height;
            if (width > height)
            {
                // Wide window
                projScaleX = height / width;
                projScaleY = 1.0f;
            }
            else
            {
                // Tall window
                projScaleX = 1.0f;
                projScaleY = width / height;
            }
            projMatrix = Matrix.CreateScale(projScaleX, projScaleY, 0.0f);
            projMatrix.M43 = 0.5f;
        }



        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>(@"fonts\MyFont");

            roundLineManager = new RoundLineManager();
            roundLineManager.Init(GraphicsDevice, Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;


            Create2DProjectionMatrix();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private static TimeSpan _lastThink;
        private static TimeSpan _lastMove;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            // Avatar
            {
                _dawnWorld.Avatar.ClearActionQueue();

                if (keyboardState.IsKeyDown(Keys.I))
                    _dawnWorld.Avatar.WalkForward();
                if (keyboardState.IsKeyDown(Keys.L))
                    _dawnWorld.Avatar.TurnLeft();
                if (keyboardState.IsKeyDown(Keys.J))
                    _dawnWorld.Avatar.TurnRight();
                if (keyboardState.IsKeyDown(Keys.Space))
                    _dawnWorld.Avatar.Attack();
            }

            // Think = Decide where to move
            if ((gameTime.TotalGameTime - _lastThink).TotalMilliseconds > 100)
            {
                _thinkTimer.Reset();
                _thinkTimer.Start();
                _dawnWorld.MoveAll();
                _thinkTimer.Stop();

                _lastThink = gameTime.TotalGameTime;
            }

            // Move
            if ((gameTime.TotalGameTime - _lastMove).TotalMilliseconds > 50)
            {
                _moveTimer.Reset();
                _moveTimer.Start();
                _dawnWorld.ApplyMove((gameTime.TotalGameTime - _lastMove).TotalMilliseconds);
                _moveTimer.Stop();
                _lastMove = gameTime.TotalGameTime;
            }

            _worldInformation = _dawnWorld.GetWorldInformation();

            if (gamePadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }


            UpdateCamera(keyboardState, gamePadState);

            UpdateDrawOptions(keyboardState);

            base.Update(gameTime);
        }

        private void UpdateDrawOptions(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.PageUp))
                roundLineManager.BlurThreshold *= 1.001f;
            if (keyboardState.IsKeyDown(Keys.PageDown))
                roundLineManager.BlurThreshold /= 1.001f;

            if (roundLineManager.BlurThreshold > 1)
                roundLineManager.BlurThreshold = 1;
        }

        private void UpdateCamera(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (gamePadState.Buttons.A == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.A))
            {
                if (!aButtonDown)
                {
                    aButtonDown = true;
                    roundLineTechniqueIndex++;
                    if (roundLineTechniqueIndex >= roundLineTechniqueNames.Length)
                        roundLineTechniqueIndex = 0;
                }
            }
            else
            {
                aButtonDown = false;
            }

            float leftX = gamePadState.ThumbSticks.Left.X;
            if (keyboardState.IsKeyDown(Keys.Left))
                leftX -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.Right))
                leftX += 1.0f;

            float leftY = gamePadState.ThumbSticks.Left.Y;
            if (keyboardState.IsKeyDown(Keys.Up))
                leftY += 1.0f;
            if (keyboardState.IsKeyDown(Keys.Down))
                leftY -= 1.0f;

            float dx = leftX * 0.01f * cameraZoom;
            float dy = leftY * 0.01f * cameraZoom;

            bool zoomIn = gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                          keyboardState.IsKeyDown(Keys.Z);
            bool zoomOut = gamePadState.Buttons.LeftShoulder == ButtonState.Pressed ||
                           keyboardState.IsKeyDown(Keys.X);

            cameraX += dx;
            cameraY += dy;
            if (zoomIn)
                cameraZoom /= 0.99f;
            if (zoomOut)
                cameraZoom *= 0.99f;

            viewMatrix = Matrix.CreateTranslation(-cameraX, -cameraY, 0) * Matrix.CreateScale(1.0f / cameraZoom, 1.0f / cameraZoom, 1.0f);
            //viewMatrix = Matrix.CreateTranslation(-(float)_avatar.Place.Position.X, -(float)_avatar.Place.Position.Y, 0) * Matrix.CreateScale(1.0f / cameraZoom, 1.0f / cameraZoom, 1.0f);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Matrix viewProjMatrix = viewMatrix * projMatrix;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            {
                float time = (float) gameTime.TotalRealTime.TotalSeconds;
                string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];
                {
                    var obstacles = _dawnWorld.Environment.GetObstacles();
                    foreach (var current in obstacles)
                    {
                        var points = current.Form.Shape.Points;
                        var pos = current.Position;

                        DrawPolygon(roundLineManager, points, viewProjMatrix, time, curTechniqueName);
                    }
                }
                {
                    var creatures = _dawnWorld.Environment.GetCreatures();
                    foreach (var current in creatures)
                    {
                        DrawCreature(current, roundLineManager, viewProjMatrix, time, curTechniqueName);
                    }
                }
            }

            spriteBatch.Begin();

            spriteBatch.DrawString(font, _worldInformation, new Vector2(100f, 100f), Color.Green);

            string technicalInformation = string.Format("Think: {0}ms; Move: {1}ms", _thinkTimer.ElapsedMilliseconds, _moveTimer.ElapsedMilliseconds);

            spriteBatch.DrawString(font, technicalInformation, new Vector2(100f, 150f), Color.Green);

            if (_dawnWorld.Avatar != null)
            {
                string stats = string.Format("Damage: {0}%", _dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled);
                spriteBatch.DrawString(font, stats, new Vector2(100f, 200f), Color.Green);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }

        private static void DrawCreature(Creature creature, RoundLineManager manager, Matrix viewProjMatrix, float time, string curTechniqueName)
        {
            Color color = Color.White;

            if (creature.Specy == CreatureType.Avatar)
                color = Color.Gold;
            if (creature.Specy == CreatureType.Predator)
                color = Color.Green;

            if (creature.IsAttacked())
                color = Color.Black;
            if (creature.IsAttacked())
                color = Color.Red;

            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;

            var vector1 = new Vector2((float)(pos.X ), (float)(pos.Y ));
            var vector2 = new Vector2((float)(pos.X + Math.Cos(angle) * 10), (float)(pos.Y + Math.Sin(angle) * 10));
            RoundLine line = new RoundLine(vector1, vector2);
            manager.Draw(line, 3, color, viewProjMatrix, time, curTechniqueName);
        }

        private static void DrawPolygon(RoundLineManager manager, IList<Vector> points, Matrix viewProjMatrix, float time, string curTechniqueName)
        {
           for (int i = 0; i < points.Count; i++)
            {
                DawnOnline.Simulation.Collision.Vector point1 = points[i];
                DawnOnline.Simulation.Collision.Vector point2;
                if (i + 1 >= points.Count)
                {
                    point2 = points[0];
                }
                else
                {
                    point2 = points[i + 1];
                }
                //var vector1 = new Vector2((float)(point1.X + position.X), (float)(point1.Y + position.Y));
                //var vector2 = new Vector2((float)(point2.X + position.X), (float)(point2.Y + position.Y));
                var vector1 = new Vector2((float)(point1.X ), (float)(point1.Y ));
                var vector2 = new Vector2((float)(point2.X ), (float)(point2.Y ));

                RoundLine line = new RoundLine(vector1, vector2);

                manager.Draw(line, 3, Color.Black, viewProjMatrix, time, curTechniqueName);
                //manager.Draw(line, 3, Color.Black, new Matrix(), time, curTechniqueName);
            }
        }

        

    }
}
