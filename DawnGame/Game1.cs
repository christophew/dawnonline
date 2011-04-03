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

        private Stopwatch _thinkTimer = new Stopwatch();
        private Stopwatch _moveTimer = new Stopwatch();
        private Stopwatch _drawTimer = new Stopwatch();
        private Stopwatch _updateTimer = new Stopwatch();
        private double _lastDrawTime = 0;


        private SpriteFont font;
        private Matrix _viewProjMatrix;

        RoundLineManager roundLineManager;
        int roundLineTechniqueIndex = 0;
        string[] roundLineTechniqueNames;

        DawnWorld _dawnWorld = new DawnWorld();

        private GameObject _creatureModel;
        private GameObject _creatureModel_Avatar;
        //private GameObject _creatureModel_Monkey;
        private GameObject _cubeModel;
        private GameObject _wallModel;
        private GameObject _bulletModel;
        private GameObject _gunModel;
        private GameObject _treasureModel;
        private GameObject _predatorFactoryModel;

        Matrix lineWorldMatrix = Matrix.CreateRotationX(MathHelper.PiOver2); // draw lines on the z-plane

        private Texture2D _wallTexture;

        private ICamera _camera;

        Effect effect;
        Texture2D cloudMap;
        Model skyDome;


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
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>(@"fonts\MyFont");

            _creatureModel = new GameObject(Content.Load<Model>(@"shark"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 10f);
            _creatureModel_Avatar = new GameObject(Content.Load<Model>(@"directx"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 10f);
            _gunModel = new GameObject(Content.Load<Model>(@"gun"), new Vector3(MathHelper.PiOver2, 0, -MathHelper.PiOver2), Vector3.Zero, 7f);

            _cubeModel = new GameObject(Content.Load<Model>(@"box"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 25f);
            _wallModel = new GameObject(Content.Load<Model>(@"brickwall"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 25f);
            _predatorFactoryModel = new GameObject(Content.Load<Model>(@"Factory4"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -50, 0), 50f);

            //_bulletModel = new GameObject(Content.Load<Model>(@"bullet"), Vector3.Zero, 1f);
            _bulletModel = new GameObject(Content.Load<Model>(@"firebullet"), Vector3.Zero, Vector3.Zero, 1.5f);

            _treasureModel = new GameObject(Content.Load<Model>(@"cube3"), Vector3.Zero, Vector3.Zero, 5f);


            _wallTexture = Content.Load<Texture2D>(@"Textures\brickThumb");


            // Skydome
            //effect = Content.Load<Effect>("Series4Effects");
            skyDome = Content.Load<Model>("dome");
            //skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();



            roundLineManager = new RoundLineManager();
            roundLineManager.Init(GraphicsDevice, Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;



            _camera = new BirdsEyeFollowCamera(GraphicsDevice, 800, 500, _dawnWorld.Avatar);
            //_camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(1500f, 2000f, 1000f), 500);
            //_camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            //_camera = new FirstPersonCamera(Window, 100);
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
        private static DateTime _creaturesAddedAt;

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

            // Avatar
            UpdateAvatar();

            // World
            if (keyboardState.IsKeyDown(Keys.P) && ((DateTime.Now - _creaturesAddedAt).TotalSeconds > 5))
            {
                _dawnWorld.AddCreatures(EntityType.Predator, 10);
                _creaturesAddedAt = DateTime.Now;
            }

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
            {
                _moveTimer.Reset();
                _moveTimer.Start();
                _dawnWorld.ApplyMove(gameTime.ElapsedGameTime.TotalMilliseconds);
                _moveTimer.Stop();
            }
            //var dt = 33;
            //if ((gameTime.TotalGameTime - _lastMove).TotalMilliseconds > dt)
            //{
            //    _moveTimer.Reset();
            //    _moveTimer.Start();
            //    //_dawnWorld.ApplyMove((gameTime.TotalGameTime - _lastMove).TotalMilliseconds);
            //    _dawnWorld.ApplyMove(dt);

            //    _moveTimer.Stop();
            //    _lastMove = gameTime.TotalGameTime;
            //}

            // Exit
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            UpdateCamera();
            _camera.Update(gameTime);



            base.Update(gameTime);

            _updateTimer.Stop();
        }

        private void UpdateAvatar()
        {
            var keyboardState = Keyboard.GetState();

            _dawnWorld.Avatar.ClearActionQueue();

            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Z))
                _dawnWorld.Avatar.WalkForward();
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                _dawnWorld.Avatar.WalkBackward();
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Q))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    _dawnWorld.Avatar.TurnLeftSlow();
                else
                    _dawnWorld.Avatar.TurnLeft();
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    _dawnWorld.Avatar.TurnRightSlow();
                else
                    _dawnWorld.Avatar.TurnRight();
            }
            if (keyboardState.IsKeyDown(Keys.A))
                _dawnWorld.Avatar.StrafeLeft();
            if (keyboardState.IsKeyDown(Keys.E))
                _dawnWorld.Avatar.StrafeRight();
            if (keyboardState.IsKeyDown(Keys.Space))
                _dawnWorld.Avatar.Fire();
            if (keyboardState.IsKeyDown(Keys.LeftControl))
                _dawnWorld.Avatar.FireRocket();

            if (keyboardState.IsKeyDown(Keys.T))
                _dawnWorld.Avatar.BuildEntity(EntityType.Turret);
        }

        private void UpdateCamera()
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.F1))
                _camera = new BirdsEyeCamera(GraphicsDevice, new Vector3(1500f, 2800f, 1000f), 500);
            if (keyboard.IsKeyDown(Keys.F2))
                _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F3))
                _camera = new BirdsEyeFollowCamera(GraphicsDevice, 800, 500, _dawnWorld.Avatar);
            if (keyboard.IsKeyDown(Keys.F4))
                _camera = new FirstPersonCamera(Window, 100);
            if (keyboard.IsKeyDown(Keys.F5))
                _camera = new AvatarCamera(GraphicsDevice, _dawnWorld.Environment.GetCreatures(EntityType.Predator)[0]);

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
                        DrawEntity(current);
                        DrawCreatureInfo(current);
                    }
                }
            }

            DrawTextInfo();
            //DrawSkyDome();

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

        private void DrawCubeWorld()
        {
            var obstacles = _dawnWorld.Environment.GetObstacles();
            foreach (var current in obstacles)
            {
                DrawEntity(current);
            }
        }

        private void DrawBullets()
        {
            var obstacles = _dawnWorld.Environment.GetBullets();
            foreach (var current in obstacles)
            {
                DrawEntity(current);
            }
        }

        private void DrawEntity(IEntity entity)
        {
            var rotation = new Vector3(0, -entity.Place.Angle, 0);
            var position = new Vector3(entity.Place.Position.X, 0f, entity.Place.Position.Y);

            switch (entity.Specy)
            {
                case EntityType.Avatar:
                    _creatureModel_Avatar.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Predator:
                    _creatureModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Turret:
                    _gunModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Box:
                    _cubeModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Wall:
                    _wallModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Treasure:
                    _treasureModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.PredatorFactory:
                    _predatorFactoryModel.DrawObject(_camera, position, rotation);
                    break;
                case EntityType.Bullet:
                    _bulletModel.DrawObject(_camera, position, rotation);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void DrawCreatureInfo(ICreature creature)
        {
            var pos = creature.Place.Position;
            var angle = creature.Place.Angle;
            Color color = creature.CanAttack() ? Color.Black : Color.Red;

            var attackMiddle = new Vector2(
                (float)(pos.X + Math.Cos(angle) * creature.CharacterSheet.MeleeRange),
                (float)(pos.Y + Math.Sin(angle) * creature.CharacterSheet.MeleeRange));

            DrawCircle(attackMiddle, creature.CharacterSheet.MeleeRange, color);
        }

        private void Draw2DWorld(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];
            {
                var obstacles = _dawnWorld.Environment.GetObstacles();
                foreach (var current in obstacles)
                {
                    var points = current.Place.Form.Shape.Points;
                    var pos = current.Place.Position;

                    DrawPolygon(points, time, curTechniqueName);
                }
            }
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

        private void DrawSkyDome()
        {
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(_camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(_camera.View);
                    currentEffect.Parameters["xProjection"].SetValue(_camera.Projection);
                    currentEffect.Parameters["xTexture0"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                }
                mesh.Draw();
            }
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }
    }
}
