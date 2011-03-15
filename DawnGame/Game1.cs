using System;
using System.Collections.Generic;
using DawnGame.Cameras;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Collision;
using DawnOnline.Simulation.Entities;
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

        private string _worldInformation = "";

        private Stopwatch _thinkTimer = new Stopwatch();
        private Stopwatch _moveTimer = new Stopwatch();
        private Stopwatch _drawTimer = new Stopwatch();
        private Stopwatch _updateTimer = new Stopwatch();
        private double _lastDrawTime = 0;


        private SpriteFont font;
        private Matrix _viewProjMatrix;

        RoundLineManager roundLineManager;
        bool aButtonDown = false;
        int roundLineTechniqueIndex = 0;
        string[] roundLineTechniqueNames;

        DawnWorld _dawnWorld = new DawnWorld();

        private Model _creatureModel;
        private Model _creatureModel_Avatar;
        private Model _creatureModel_Monkey;
        private Model _cubeModel;
        private Model _bulletModel;
        private Model _gunModel;

        Random _randomize = new Random();

        VertexDeclaration vertexDeclaration;

        Matrix lineWorldMatrix = Matrix.CreateRotationX(MathHelper.PiOver2); // draw lines on the z-plane

        private Texture2D _wallTexture;

        private ICamera _camera;

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


            base.Initialize();
        }

        /// <summary>
        /// Create a simple 2D projection matrix
        /// </summary>
        private Matrix Create2DProjectionMatrix()
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
            var projMatrix2D = Matrix.CreateScale(projScaleX, projScaleY, 0f);
            //projMatrix2D = Matrix.CreateScale(projScaleX, 0f, projScaleY);
            projMatrix2D.M43 = 0.5f;

            return projMatrix2D;
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
            _cubeModel = Content.Load<Model>(@"brickwall");
            //_bulletModel = Content.Load<Model>(@"bullet");
            _bulletModel = Content.Load<Model>(@"firebullet");
            _gunModel = Content.Load<Model>(@"gun");
            _creatureModel_Avatar = Content.Load<Model>(@"directx");

            _wallTexture = Content.Load<Texture2D>(@"Textures\brickThumb");


            roundLineManager = new RoundLineManager();
            roundLineManager.Init(GraphicsDevice, Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;

            _wallManager = new WallManager(GraphicsDevice, _wallTexture);


            _camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(1500f, 2000f, 1000f), 500);
            //_camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            //_camera = new FirstPersonCamera(Window, 100);


            //Create2DProjectionMatrix();

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

            _camera.Update(gameTime);
            
            KeyboardState keyboardState = Keyboard.GetState();

            // Avatar
            UpdateAvatar();

            // Think = Decide where to move
            if ((gameTime.TotalGameTime - _lastThink).TotalMilliseconds > 0)
            {
                _thinkTimer.Reset();
                _thinkTimer.Start();
                _dawnWorld.MoveAll();
                _thinkTimer.Stop();

                _lastThink = gameTime.TotalGameTime;
            }

            // Move
            if ((gameTime.TotalGameTime - _lastMove).TotalMilliseconds > 0)
            {
                _moveTimer.Reset();
                _moveTimer.Start();
                //_dawnWorld.ApplyMove((gameTime.TotalGameTime - _lastMove).TotalMilliseconds);
                _dawnWorld.ApplyMove(gameTime.ElapsedGameTime.TotalMilliseconds);

                _moveTimer.Stop();
                _lastMove = gameTime.TotalGameTime;
            }

            _worldInformation = _dawnWorld.GetWorldInformation();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            UpdateRoundingTechnique(keyboardState);

            UpdateCamera();

            UpdateDrawOptions(keyboardState);

            //UpdateEffects();

            base.Update(gameTime);

            _updateTimer.Stop();
        }

        private void UpdateAvatar()
        {
            var keyboardState = Keyboard.GetState();

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
                _dawnWorld.Avatar.Fire();
        }

        private void UpdateCamera()
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.F1))
                _camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(1500f, 2000f, 1000f), 500);
            if (keyboard.IsKeyDown(Keys.F2))
                _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F3))
                _camera = new BirdsEyeFollowCamera(GraphicsDevice, 800, 500, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F4))
                _camera = new FirstPersonCamera(Window, 100);

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


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _drawTimer.Reset();
            _drawTimer.Start();

            _viewProjMatrix = _camera.View*_camera.Projection;


            GraphicsDevice.Clear(Color.AntiqueWhite);


            {
                //Draw2DWorld(gameTime);
                //Draw3DWorld();
                DrawCubeWorld();
                DrawBullets();

                // Draw creatures
                {
                    var creatures = _dawnWorld.Environment.GetCreatures();
                    foreach (var current in creatures)
                    {
                        //DrawCreature(current, roundLineManager, viewProjMatrix, time, curTechniqueName);
                        DrawCreature(current);
                    }
                }
            }

            DrawTextInfo();


            base.Draw(gameTime);

            _drawTimer.Stop();
            _lastDrawTime = _drawTimer.ElapsedMilliseconds;
        }

        private void DrawTextInfo()
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(font, _worldInformation, new Vector2(100f, 100f), Color.Green);

            string technicalInformation = string.Format("Think: {0:0000}ms; Move: {1:0000}ms; Update: {2:0000}ms; Draw: {3:0000}ms",
                                                        _thinkTimer.ElapsedMilliseconds, _moveTimer.ElapsedMilliseconds, _updateTimer.ElapsedMilliseconds, _lastDrawTime);
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


        private void Draw2DWorld(GameTime gameTime)
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
        }

        private void Draw3DWorld()
        {
            Create3DWorld();

            //foreach (var shape in _worldShapes)
            //{
            //    shape.Draw(GraphicsDevice, _camera.View, _camera.Projection);
            //}
            _wallManager.Draw(_worldShapes, GraphicsDevice, _camera.View, _camera.Projection);
        }

        private void DrawCubeWorld()
        {
            var obstacles = _dawnWorld.Environment.GetObstacles();
            foreach (var current in obstacles)
            {
                DrawCube(current);
            }
        }

        private void DrawBullets()
        {
            var obstacles = _dawnWorld.Environment.GetBullets();
            foreach (var current in obstacles)
            {
                DrawBullet(current);
            }
        }

        private List<BasicShape> _worldShapes;
        private WallManager _wallManager;

        private void Create3DWorld()
        {
            if (_worldShapes != null)
                return;

            _worldShapes = new List<BasicShape>();

            var obstacles = _dawnWorld.Environment.GetObstacles();
            foreach (var current in obstacles)
            {
                _worldShapes.AddRange(Create3DShape(current.Form.Shape, current.Position));
            }
        }

        

        private List<BasicShape> Create3DShape(IPolygon polygon, Vector2 position)
        {
            var shape = new List<BasicShape>();
            //List<Line> lines = new List<Line>();

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                DawnOnline.Simulation.Collision.Vector point1 = polygon.Points[i];
                DawnOnline.Simulation.Collision.Vector point2;
                if (i + 1 >= polygon.Points.Count)
                {
                    point2 = polygon.Points[0];
                }
                else
                {
                    point2 = polygon.Points[i + 1];
                }

                var vector1 = new Vector2((float)(point1.X), (float)(point1.Y));
                var vector2 = new Vector2((float)(point2.X), (float)(point2.Y));

                // Only take into account straight angles
                bool turn = (point2.X == point1.X);
                var length = turn ? point2.Y - point1.Y : point2.X - point1.X;

                var wall = new BasicShape(
                    new Vector3(length, 20, 0.2f),
                    new Vector3((float)point1.X, 0, (float)point1.Y), 
                    turn ? MathHelper.PiOver2 : 0);
                wall.shapeTexture = _wallTexture;
                shape.Add(wall);
            }
            return shape;
        }

        private void DrawCreature(IEntity creature)
        {
            GameObject gameCreature = new GameObject();

            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;

            // MathHelper.PiOver2 correction => geen idee waarom mijn meshes dit nodig hebben, maar ja...
            gameCreature.rotation = new Vector3(MathHelper.PiOver2, -(float)angle, 0);
            gameCreature.position = new Vector3((float)(pos.X), 0f, (float)(pos.Y));
            gameCreature.scale = 10f;

            switch (creature.Specy)
            {
                case CreatureType.Avatar:
                    gameCreature.model = _creatureModel_Avatar;
                    break;
                case CreatureType.Predator:
                    gameCreature.model = _creatureModel;
                    break;
                case CreatureType.Turret:
                    gameCreature.model = _gunModel;
                    // MathHelper.PiOver2 correction => geen idee waarom mijn meshes dit nodig hebben, maar ja...
                    gameCreature.rotation = new Vector3(MathHelper.PiOver2, -angle, -MathHelper.PiOver2);
                    gameCreature.scale = 7f;
                   break;
                default:
                    gameCreature.model = _creatureModel;
                    break;
            }


            gameCreature.DrawObject(_camera.View, _camera.Projection);

            Color color = creature.CanAttack() ? Color.Black : Color.Red;

            var attackMiddle = new Vector2(
                (float)(pos.X + Math.Cos(angle) * creature.CharacterSheet.MeleeRange),
                (float)(pos.Y + Math.Sin(angle) * creature.CharacterSheet.MeleeRange));

            DrawCircle(attackMiddle, creature.CharacterSheet.MeleeRange, color);
        }

        private void DrawCube(Placement placement)
        {
            DrawGameObject(placement, _cubeModel, 25f);
        }

        private void DrawBullet(Bullet bullet)
        {
            DrawGameObject(bullet.Placement, _bulletModel, 2f);
        }

        private void DrawGameObject(Placement placement, Model model, float scale)
        {
            var gamePlacement = new GameObject();

            var pos = placement.Position;
            var angle = placement.Angle;

            gamePlacement.model = model;
            gamePlacement.position = new Vector3(pos.X, 0f, pos.Y);
            // MathHelper.PiOver2 correction => geen idee waarom mijn meshes dit nodig hebben, maar ja...
            gamePlacement.rotation = new Vector3(MathHelper.PiOver2, -(float)angle, 0);
            gamePlacement.scale = scale;

            gamePlacement.DrawObject(_camera.View, _camera.Projection);
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

        #endregion
    }
}
