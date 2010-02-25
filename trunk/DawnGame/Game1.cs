using System;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace DawnGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private readonly IEnvironment _environment = SimulationFactory.CreateEnvironment();
        private const int MaxX = 3000;
        private const int MaxY = 2000;
        private ICreature _avatar = SimulationFactory.CreateAvatar();

        private Texture2D oneForAll;

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
            // TODO: Add your initialization logic here
            BuildWorld();
            AddCreatures(CreatureType.Rabbit, 200);
            AddCreatures(CreatureType.Plant, 200);
            AddCreatures(CreatureType.Predator, 200);

            graphics.PreferredBackBufferWidth = 3000;
            graphics.PreferredBackBufferHeight = 2000;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            System.Windows.Forms.Control form = System.Windows.Forms.Form.FromHandle(this.Window.Handle);
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
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            oneForAll = Content.Load<Texture2D>(@"sprites\cannonball");
            //oneForAll = new Texture2D(GraphicsDevice, 1, 1);
            
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
        private static TimeSpan lastMove;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if ((gameTime.TotalGameTime - lastMove).TotalMilliseconds > 200)
            {
                MoveAll();
                lastMove = gameTime.TotalGameTime;
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            var creatures = _environment.GetCreatures();
            foreach (var current in creatures)
            {
                //DrawCreature(current);
                spriteBatch.Draw(oneForAll, new Vector2((float)current.Place.Position.X, (float)current.Place.Position.Y), Color.White);
            }

            var obstacles = _environment.GetObstacles();
            foreach (var current in obstacles)
            {
                //DrawObstacle(current);
                spriteBatch.Draw(oneForAll, new Vector2((float)current.Position.X, (float)current.Position.Y), Color.Green);
            }

            spriteBatch.End();

            base.Draw(gameTime);
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

                //var killed = current.Attack();
                //if ((killed != null) && (killed != _avatar))
                //    _environment.KillCreature(killed);

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

            //Info.Content = string.Format("Plant: {0}; Rabbits: {1}; Predators:{2}", nrOfPlants, nrOfRabbits, nrOfPredators);
        }


    }
}
