using System;
using System.Collections.Generic;
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
        Matrix projMatrix2D;
        Matrix viewMatrix;
        private Matrix _viewProjMatrix;

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

        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;

        Matrix lineWorldMatrix = Matrix.CreateRotationX(MathHelper.PiOver2); // draw lines on the z-plane

        private Texture2D _wallTexture;

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

            //StrokeFont.AddStringCentered("Microbe\nPatr l", testLineList);


            base.Initialize();
        }

        /// <summary>
        /// Create a simple 2D projection matrix
        /// </summary>
        private void Create2DProjectionMatrix()
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
            projMatrix2D = Matrix.CreateScale(projScaleX, projScaleY, 0f);
            //projMatrix2D = Matrix.CreateScale(projScaleX, 0f, projScaleY);
            projMatrix2D.M43 = 0.5f;
        }

        private void Create3DProjectionMatrix()
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
            _creatureModel = Content.Load<Model>(@"shark");
            //_creatureModel = Content.Load<Model>(@"launcher_head");
            _creatureModel_Avatar = Content.Load<Model>(@"directx");

            _wallTexture = Content.Load<Texture2D>(@"Textures\brickThumb");


            roundLineManager = new RoundLineManager();
            roundLineManager.Init(GraphicsDevice, Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;

            Create2DProjectionMatrix();
            Create3DProjectionMatrix();
            UpdateViewMatrix();

            //InitializeEffect();
            //InitializePointList();
            //InitializeLineList();
            //InitializeLineStrip();
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
                if (keyboardState.IsKeyDown(Keys.Left))
                    _dawnWorld.Avatar.TurnLeft();
                if (keyboardState.IsKeyDown(Keys.Right))
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

            UpdateCamera(keyboardState);
            //UpdateCamera_FirstPerson(_dawnWorld.Avatar);

            UpdateDrawOptions(keyboardState);

            //UpdateEffects();

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

        Vector3 cameraPosition = new Vector3(1500f, 2000f, 1000f);
        //Vector3 cameraPosition = new Vector3(0, 0, 5);
        private float pan = 0;

        void UpdateCamera(KeyboardState keyboardState)
        {
            float cameraVelocity = 10f;

            // Left/Right
            if (keyboardState.IsKeyDown(Keys.NumPad4))
                cameraPosition.X -= cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad6))
                cameraPosition.X += cameraVelocity;

            // Up/Down
            if (keyboardState.IsKeyDown(Keys.NumPad8))
                cameraPosition.Z -= cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad2))
                cameraPosition.Z += cameraVelocity;

            // In/Out
            if (keyboardState.IsKeyDown(Keys.NumPad7))
                cameraPosition.Y += cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad9))
                cameraPosition.Y -= cameraVelocity;

            // Pan
            if (keyboardState.IsKeyDown(Keys.NumPad1))
                pan += cameraVelocity;
            if (keyboardState.IsKeyDown(Keys.NumPad3))
                pan -= cameraVelocity;

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {

            Vector3 cameraLookAt = new Vector3(cameraPosition.X, 0f, cameraPosition.Z + pan);

            //Vector3 cameraLookAt = Vector3.Zero;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Forward);
        }

        void UpdateCamera_FirstPerson(Creature creature)
        {
            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;

            var camPosition = new Vector3((float)(pos.X), 20, (float)(pos.Y));
            var cameraLookAt = new Vector3((float)(pos.X + Math.Cos(angle) * 10), 17, (float)(pos.Y + Math.Sin(angle) * 10));

            viewMatrix = Matrix.CreateLookAt(camPosition, cameraLookAt, Vector3.Up);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _drawTimer.Reset();
            _drawTimer.Start();

            _viewProjMatrix = viewMatrix * projMatrix;


            GraphicsDevice.Clear(Color.AntiqueWhite);


            {
                float time = (float)gameTime.TotalGameTime.TotalSeconds;
                string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];
                {
                    var obstacles = _dawnWorld.Environment.GetObstacles();
                    foreach (var current in obstacles)
                    {
                        var points = current.Form.Shape.Points;
                        var pos = current.Position;

                        DrawPolygon(points, time, curTechniqueName);
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

            string technicalInformation = string.Format("Think: {0}ms; Move: {1}ms; Update: {2}ms; Draw: {3}ms",
                _thinkTimer.ElapsedMilliseconds, _moveTimer.ElapsedMilliseconds, _updateTimer.ElapsedMilliseconds, _lastDrawTime);
            spriteBatch.DrawString(font, technicalInformation, new Vector2(100f, 150f), Color.Green);

            if (_dawnWorld.Avatar != null)
            {
                string stats = string.Format("Damage: {0}%", _dawnWorld.Avatar.CharacterSheet.Damage.PercentFilled);
                spriteBatch.DrawString(font, stats, new Vector2(100f, 200f), Color.Green);
            }

            spriteBatch.DrawString(font, string.Format("Camera position: ({0}, {1}, {2}); pan: {3}", cameraPosition.X, cameraPosition.Y, cameraPosition.Z, pan), new Vector2(100f, 250f), Color.Green);

            spriteBatch.End();


            // Custom draw test
            //graphics.GraphicsDevice.VertexDeclaration = vertexDeclaration;
            //basicEffect.Begin();
            //foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            //{
            //    pass.Begin();
            //    DrawLineList();
            //    pass.End();
            //}
            //basicEffect.End();
            // End custom draw test

            {
                float tilt = MathHelper.ToRadians(22.5f);
                angle += 0.5f;
                var worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(angle));
                cube.shapeTexture = _wallTexture;
                cubeEffect = new BasicEffect(GraphicsDevice);
                cubeEffect.World = worldMatrix;
                cubeEffect.View = viewMatrix;
                cubeEffect.Projection = projMatrix;
                cubeEffect.TextureEnabled = true;
                //cubeEffect.EmissiveColor = (Vector3)Color.White;
                DrawCube(cube, worldMatrix);
            }

            base.Draw(gameTime);

            _drawTimer.Stop();
            _lastDrawTime = _drawTimer.ElapsedMilliseconds;
        }

        BasicShape cube = new BasicShape(new Vector3(1000, 10, 2), new Vector3(500, 100, 500));
        BasicEffect cubeEffect;
        private float angle;

        private void DrawCube(BasicShape cube, Matrix world)
        {
            cubeEffect.World = world;

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                cubeEffect.Texture = cube.shapeTexture;
                cube.RenderShape(GraphicsDevice);
            }
        }

        private void DrawCreature(Creature creature)
        {
            GameObject gameCreature = new GameObject();

            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;
           

            gameCreature.model = creature.Equals(_dawnWorld.Avatar)? _creatureModel_Avatar : _creatureModel;
            gameCreature.position = new Vector3((float)(pos.X), 0f, (float)(pos.Y));
            gameCreature.rotation = new Vector3(0, -(float)angle, 0);
            gameCreature.scale = 10f;

            gameCreature.DrawObject(viewMatrix, projMatrix);

            Color color = creature.CanAttack() ? Color.Black : Color.Red;

            var attackMiddle = new Vector2(
                (float)(pos.X + Math.Cos(angle) * creature.CharacterSheet.MeleeRange),
                (float)(pos.Y + Math.Sin(angle) * creature.CharacterSheet.MeleeRange));

            DrawCircle(attackMiddle, creature.CharacterSheet.MeleeRange, color);
        }

        private void DrawPolygon(IList<Vector> points, float time, string curTechniqueName)
        {
            List<RoundLine> lines = new List<RoundLine>();
            //List<Line> lines = new List<Line>();
            
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
                //Line line = new Line(vector1, vector2);
                lines.Add(line);
            }

            roundLineManager.Draw(lines, 3, Color.Black, lineWorldMatrix * _viewProjMatrix, time, curTechniqueName);
            //lineManager.Draw(lines, 3, Color.Black.ToVector4(), viewMatrix, projMatrix, time, null, lineWorldMatrix, 0.97f);
        }

        private void DrawCircle(Vector2 centre, double radius, Color color)
        {
            float time = (float)1f/60f;
            string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];

            const int nrOfVertexes = 32;

            List<RoundLine> lines = new List<RoundLine>();
            //List<Line> lines = new List<Line>();

            Vector2 lastPoint = new Vector2((float)(centre.X + radius), (float)(centre.Y));

            for (int i = 1; i < nrOfVertexes + 1; i++)
            {
                float currentAngle = i*MathHelper.TwoPi/nrOfVertexes;

                var newPoint = new Vector2((float)(centre.X + Math.Cos(currentAngle) * radius), (float)(centre.Y + Math.Sin(currentAngle) * radius));

                RoundLine line = new RoundLine(lastPoint, newPoint);
                //Line line = new Line(lastPoint, newPoint);

                lines.Add(line);

                lastPoint = newPoint;
            }

            roundLineManager.Draw(lines, 1, color, lineWorldMatrix * _viewProjMatrix, time, curTechniqueName);
            //lineManager.Draw(lines, 1, color.ToVector4(), viewMatrix, projMatrix, time, null, lineWorldMatrix, 1);
        }

        #region CustomDraw test

        int points = 8;
        VertexPositionNormalTexture[] pointList;
        VertexBuffer vertexBuffer;

        short[] lineListIndices;
        short[] lineStripIndices;

 
        private void InitializeLineList()
        {
            // Initialize an array of indices of type short.
            lineListIndices = new short[(points * 2)];

            // Populate the array with references to indices in the vertex buffer
            for (int i = 0; i < points; i++)
            {
                lineListIndices[i * 2] = (short)(i + 1);
                lineListIndices[(i * 2) + 1] = (short)(i + 2);
            }

            lineListIndices[(points * 2) - 1] = 1;

        }

        private void InitializeLineStrip()
        {
            // Initialize an array of indices of type short.
            lineStripIndices = new short[points + 1];

            // Populate the array with references to indices in the vertex buffer.
            for (int i = 0; i < points; i++)
            {
                lineStripIndices[i] = (short)(i + 1);
            }

            lineStripIndices[points] = 1;

        }

        private void DrawLineList()
        {
            graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                PrimitiveType.LineList,
                pointList,
                0,  // vertex buffer offset to add to each element of the index buffer
                9,  // number of vertices in pointList
                lineListIndices,  // the index buffer
                0,  // first index element to read
                8   // number of primitives to draw
            );

        }

        private void DrawLineStrip()
        {
            graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                PrimitiveType.LineStrip,
                pointList,
                0,   // vertex buffer offset to add to each element of the index buffer
                9,   // number of vertices to draw
                lineStripIndices,
                0,   // first index element to read
                8    // number of primitives to draw
            );
        }

        private void UpdateEffects()
        {
            if (basicEffect != null)
            {
                basicEffect.View = viewMatrix;
                basicEffect.Projection = projMatrix;
            }
        }


        #endregion
    }
}
