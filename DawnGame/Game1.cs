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
        private Stopwatch _drawTimer = new Stopwatch();
        private Stopwatch _updateTimer = new Stopwatch();
        private double _lastDrawTime = 0;


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

        private Model _creatureModel;
        private Model _creatureModel_Avatar;
        private Model _creatureModel_Monkey;

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
            projMatrix = Matrix.CreateScale(projScaleX, projScaleY, 0f);
            projMatrix.M43 = 0.5f;
        }

        public void Create3DProjectionMatrix()
        {
            projMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                1f,
                50000f);

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
            _creatureModel = Content.Load<Model>(@"cube");
            _creatureModel_Monkey = Content.Load<Model>(@"Monkey");
            _creatureModel_Avatar = Content.Load<Model>(@"directx");


            roundLineManager = new RoundLineManager();
            roundLineManager.Init(GraphicsDevice, Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;


            //Create2DProjectionMatrix();
            Create3DProjectionMatrix();
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
            _updateTimer.Reset();
            _updateTimer.Start();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyboardState = Keyboard.GetState();

            // Avatar
            {
                _dawnWorld.Avatar.ClearActionQueue();

                if (keyboardState.IsKeyDown(Keys.Up))
                    _dawnWorld.Avatar.WalkForward();
                if (keyboardState.IsKeyDown(Keys.Down))
                    _dawnWorld.Avatar.WalkBackward();
                if (keyboardState.IsKeyDown(Keys.Right)) // OK OK, left and right are inversed.. so?
                    _dawnWorld.Avatar.TurnLeft();
                if (keyboardState.IsKeyDown(Keys.Left)) // OK OK, left and right are inversed.. so?
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

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            UpdateRoundingTechnique(keyboardState);

            //UpdateCamera(keyboardState);
            UpdateCamera2(keyboardState);
            //UpdateCamera_FirstPerson(_dawnWorld.Avatar);

            UpdateDrawOptions(keyboardState);

            base.Update(gameTime);

            _updateTimer.Stop();
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

        private void UpdateRoundingTechnique(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.A))
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
        }

        Vector3 cameraPosition = new Vector3(1500f, 1000f, 2000f);
        private float pan = 0f;

        void UpdateCamera2(KeyboardState keyboardState)
        {
            float cameraVelocity = 10f;

            // Left/Right
            if (keyboardState.IsKeyDown(Keys.NumPad4))
                cameraPosition.X -= cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad6))
                cameraPosition.X += cameraVelocity;

            // Up/Down
            if (keyboardState.IsKeyDown(Keys.NumPad8))
                cameraPosition.Y += cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad2))
                cameraPosition.Y -= cameraVelocity;

            // In/Out
            if (keyboardState.IsKeyDown(Keys.NumPad7))
                cameraPosition.Z += cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad9))
                cameraPosition.Z -= cameraVelocity;

            // Pan
            if (keyboardState.IsKeyDown(Keys.NumPad1))
                pan += cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad3))
                pan -= cameraVelocity;

            
            Vector3 cameraLookAt = new Vector3(cameraPosition.X, cameraPosition.Y + pan, 0f);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
        }

        void UpdateCamera_FirstPerson(Creature creature)
        {
            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;

            var normalizedAngle = MathHelper.WrapAngle((float)angle);
            var correction = MathHelper.PiOver2;
            if (normalizedAngle > MathHelper.PiOver2 || normalizedAngle < -MathHelper.PiOver2)
                correction = -MathHelper.PiOver2;
            Matrix cameraRotation = Matrix.CreateRotationZ(correction);

            var camPosition = new Vector3((float)(pos.X), (float)(pos.Y), 20);
            var cameraLookAt = new Vector3((float)(pos.X + Math.Cos(angle) * 10), (float)(pos.Y + Math.Sin(angle) * 10), 20);

            viewMatrix = Matrix.CreateLookAt(camPosition, cameraLookAt, Vector3.Up) * cameraRotation;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _drawTimer.Reset();
            _drawTimer.Start();

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
                        //DrawCreature(current, roundLineManager, viewProjMatrix, time, curTechniqueName);
                        DrawCreature(current);
                    }
                }
            }

            spriteBatch.Begin();

            spriteBatch.DrawString(font, _worldInformation, new Vector2(100f, 100f), Color.Green);

            string technicalInformation = string.Format("Think: {0}ms; Move: {1}ms; Draw: {2}ms; Update: {3}ms", 
                _thinkTimer.ElapsedMilliseconds, _moveTimer.ElapsedMilliseconds, _updateTimer.ElapsedMilliseconds, _lastDrawTime);
            spriteBatch.DrawString(font, technicalInformation, new Vector2(100f, 150f), Color.Green);

            if (_dawnWorld.Avatar != null)
            {
                string stats = string.Format("Damage: {0}%", _dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled);
                spriteBatch.DrawString(font, stats, new Vector2(100f, 200f), Color.Green);
            }

            spriteBatch.End();


            base.Draw(gameTime);

            _drawTimer.Stop();
            _lastDrawTime = _drawTimer.ElapsedMilliseconds;
        }

        private void DrawCreature(Creature creature)
        {
            GameObject gameCreature = new GameObject();

            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;
           

            gameCreature.model = creature.Equals(_dawnWorld.Avatar)? _creatureModel_Avatar : _creatureModel;
            gameCreature.position = new Vector3((float)(pos.X), (float)(pos.Y), 0f);
            gameCreature.rotation = new Vector3(0, 0, (float)angle);
            gameCreature.scale = 10f;

            DrawObject(gameCreature);
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

            // Draw body
            var pos = creature.Place.Position;

            // Draw direction
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

        void DrawObject(GameObject gameObject)
        {
            foreach (var mesh in gameObject.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World = Matrix.CreateTranslation(gameObject.position);

                    effect.World = Matrix.CreateFromYawPitchRoll(
                                       gameObject.rotation.Y,
                                       gameObject.rotation.X,
                                       gameObject.rotation.Z) *
                                   Matrix.CreateScale(gameObject.scale) *
                                   Matrix.CreateTranslation(gameObject.position);


                    effect.Projection = projMatrix;
                    effect.View = viewMatrix;
                }
                mesh.Draw();
            }
        }


    }
}
