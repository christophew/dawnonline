using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DawnGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        LineBatch lineBatch;

        private readonly IEnvironment _environment = SimulationFactory.CreateEnvironment();
        private const int MaxX = 3000;
        private const int MaxY = 2000;
        private ICreature _avatar = SimulationFactory.CreateAvatar();

        private string Information = "";
        private SpriteFont font;
        Matrix projMatrix;
        Matrix viewMatrix;
        Matrix viewMatrix2;

        private Texture2D oneForAll;

        RoundLineManager roundLineManager;
        float cameraX = 1500;
        float cameraY = 1000;
        float cameraZoom = 1000;
        bool aButtonDown = false;
        int roundLineTechniqueIndex = 0;
        string[] roundLineTechniqueNames;


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
            _environment.AddCreature(_avatar,
                                     new Coordinate { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) }, 0);
            BuildWorld();

            AddCreatures(CreatureType.Rabbit, 300);
            AddCreatures(CreatureType.Plant, 300);
            AddCreatures(CreatureType.Predator, 100);

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
            lineBatch = new LineBatch(GraphicsDevice, 1f);

            // TODO: use this.Content to load your game content here
            oneForAll = Content.Load<Texture2D>(@"sprites\creature");
            //oneForAll = Content.Load<Texture2D>(@"sprites\cannonball");
            //oneForAll = new Texture2D(GraphicsDevice, 1, 1);

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


            if ((gameTime.TotalGameTime - _lastMove).TotalMilliseconds > 100)
            {
                if (keyboardState.IsKeyDown(Keys.I))
                    _avatar.WalkForward();
                if (keyboardState.IsKeyDown(Keys.L))
                    _avatar.TurnLeft();
                if (keyboardState.IsKeyDown(Keys.J))
                    _avatar.TurnRight();

                MoveAll();
                _lastMove = gameTime.TotalGameTime;
            }

            if (gamePadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

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

            if (keyboardState.IsKeyDown(Keys.PageUp))
                roundLineManager.BlurThreshold *= 1.001f;
            if (keyboardState.IsKeyDown(Keys.PageDown))
                roundLineManager.BlurThreshold /= 1.001f;

            if (roundLineManager.BlurThreshold > 1)
                roundLineManager.BlurThreshold = 1;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Matrix viewProjMatrix = viewMatrix * projMatrix;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //lineBatch.Begin();
            //{
            //    //DrawTestPolygon(lineBatch);

            //    //var obstacles = _environment.GetObstacles();
            //    //foreach (var current in obstacles)
            //    //{
            //    //    var points = current.Form.Shape.Points;
            //    //    var pos = current.Position;

            //    //    DrawPolygon(lineBatch, points, pos);
            //    //}
            //}
            //lineBatch.End();

            {
                float time = (float) gameTime.TotalRealTime.TotalSeconds;
                string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];
                {
                    var obstacles = _environment.GetObstacles();
                    foreach (var current in obstacles)
                    {
                        var points = current.Form.Shape.Points;
                        var pos = current.Position;

                        DrawPolygon(roundLineManager, points, viewProjMatrix, time, curTechniqueName);
                    }
                }
                {
                    var creatures = _environment.GetCreatures();
                    foreach (var current in creatures)
                    {
                        //DrawCreature(current);
                        var color = Color.White;
                        if (current.Specy == CreatureType.Plant)
                            color = Color.Green;
                        if (current.Specy == CreatureType.Rabbit)
                            color = Color.Blue;
                        if (current.Specy == CreatureType.Predator)
                            color = Color.Red;
                        if (current.Specy == CreatureType.Avatar)
                            color = Color.Gold;

                        DrawCreature(current, color, roundLineManager, viewProjMatrix, time, curTechniqueName);
                    }
                }
            }

            spriteBatch.Begin();
            //DrawSprites(spriteBatch);
            spriteBatch.DrawString(font, Information, new Vector2(100f, 100f), Color.Green);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        private void DrawSprites(SpriteBatch spriteBatch)
        {
            var creatures = _environment.GetCreatures();
            foreach (var current in creatures)
            {
                //DrawCreature(current);
                var color = Color.White;
                if (current.Specy == CreatureType.Plant)
                    color = Color.Green;
                if (current.Specy == CreatureType.Rabbit)
                    color = Color.Blue;
                if (current.Specy == CreatureType.Predator)
                    color = Color.Red;

                //spriteBatch.Draw(
                //    oneForAll, 
                //    new Vector2((float)current.Place.Position.X, (float)current.Place.Position.Y), 
                //    color);


                //Rectangle destination = new Rectangle(0, 0, 10, 10);

                spriteBatch.Draw(
                    oneForAll,
                    new Vector2((float)current.Place.Position.X, (float)current.Place.Position.Y),
                    null,
                    color,
                    (float)current.Place.Angle,
                    Vector2.Zero, // rotation origin, 
                    1f, //1.0f / this.cameraZoom, 
                    SpriteEffects.None,
                    0);
            }
        }

        private static void DrawCreature(ICreature creature, Color color, RoundLineManager manager, Matrix viewProjMatrix, float time, string curTechniqueName)
        {
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

        private void BuildWorld()
        {
            // World boundaries
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, -20), new Coordinate { X = MaxX / 2.0, Y = -11 }); // Top
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(MaxX, 20), new Coordinate { X = MaxX / 2.0, Y = MaxY + 11 }); // Bottom
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(-20, MaxY), new Coordinate { X = -11, Y = MaxY / 2.0 }); // Left
            _environment.AddObstacle(SimulationFactory.CreateObstacleBox(20, MaxY), new Coordinate { X = MaxX + 11, Y = MaxY / 2.0 }); // Right

            // Randow obstacles
            int maxHeight = 200;
            int maxWide = 200;
            for (int i = 0; i < 50; )
            {
                int height = _randomize.Next(maxHeight);
                int wide = _randomize.Next(maxWide);
                var position = new Coordinate(_randomize.Next(MaxX - wide), _randomize.Next(MaxY - height));
                var box = SimulationFactory.CreateObstacleBox(wide, height);

                if (_environment.AddObstacle(box, position))
                    i++;
            }
        }

        private void AddCreatures(CreatureType specy, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _environment.AddCreature(SimulationFactory.CreateCreature(specy),
                                 new Coordinate { X = _randomize.Next(MaxX), Y = _randomize.Next(MaxY) },
                                 _randomize.Next(6));
            }
        }

        private void MoveAll()
        {
            var creatures = new List<ICreature>(_environment.GetCreatures());

            int nrOfPlants = 0;
            int nrOfRabbits = 0;
            int nrOfPredators = 0;

            foreach (var current in creatures)
            {
                if (!current.Alive)
                    continue;

                current.Move();

                // Died of old age..
                if (!current.Alive)
                    continue;

                var killed = current.Attack();
                if ((killed != null) && (killed != _avatar))
                    _environment.KillCreature(killed);

                if (current.Specy == CreatureType.Plant) nrOfPlants++;
                if (current.Specy == CreatureType.Rabbit) nrOfRabbits++;
                if (current.Specy == CreatureType.Predator) nrOfPredators++;

                // TEST: grow on organic waste
                //if ((killed != null) && (killed.Specy != CreatureType.Plant))
                //{
                //    if (_randomize.Next(5) == 0)
                //    {
                //        var plant = SimulationFactory.CreatePlant();
                //        _environment.AddCreature(plant, killed.Place.Position, 0);
                //    }
                //}
            }

            // Repopulate
            //{
            //    if (nrOfPlants == 0) AddCreatures(CreatureType.Plant, 10);
            //    if (nrOfPredators == 0) AddCreatures(CreatureType.Predator, 10);
            //    if (nrOfRabbits == 0) AddCreatures(CreatureType.Rabbit, 10);
            //}

            Information = string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
        }


    }
}
