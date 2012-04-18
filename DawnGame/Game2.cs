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
    public class Game2 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Viewport defaultViewport;
        Viewport leftViewport;
        Viewport rightViewport;


        public Game2()
        {
            graphics = new GraphicsDeviceManager(this);

            //HACK: for showing acurate fps counts, always remove these lines when releasing!
            //{
            //    IsFixedTimeStep = false;
            //    graphics.SynchronizeWithVerticalRetrace = false;
            //}


            Content.RootDirectory = "Content";

            //Components.Add(new DefaultGameBehaviour(this));
            Components.Add(new DeferredRenderer(this));
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
            // Viewports
            defaultViewport = graphics.GraphicsDevice.Viewport;
            leftViewport = defaultViewport;
            rightViewport = defaultViewport;
            leftViewport.Width = leftViewport.Width / 2;
            rightViewport.Width = rightViewport.Width / 2;
            rightViewport.X = leftViewport.Width;

            // Default to leftViewport
            graphics.GraphicsDevice.Viewport = leftViewport;
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
            KeyboardState keyboardState = Keyboard.GetState();

            // Exit
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            base.Update(gameTime);


            // FPS
            Console.WriteLine("Update ElapsedGameTime: " + gameTime.ElapsedGameTime.TotalMilliseconds);
            Console.WriteLine("Update per second: " + 1.0 / gameTime.ElapsedGameTime.TotalSeconds);
            this.Window.Title = "Dawn: " + (int)(1.0/gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }

    }
}
