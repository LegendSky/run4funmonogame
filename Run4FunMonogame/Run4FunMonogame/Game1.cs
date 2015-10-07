using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Run4FunMonogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D star;
        private KeyboardState keyState;
        private Vector2 position, velocity;
        private float speed = 30;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

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
            keyState = Keyboard.GetState();
            position = new Vector2(300, 300);
            velocity = Vector2.Zero;

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
            star = Content.Load<Texture2D>("player");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            star.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            keyState = Keyboard.GetState();

            // Exit button.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();

            Console.WriteLine("" + (GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5).ToString());

            Console.WriteLine(GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5);

            bool triggerLeftPressed = GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5;
            bool triggerRightPressed = GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.5;
            bool leftShoulderPressed = GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed;

            // Move.
            if (keyState.IsKeyDown(Keys.Up) || triggerLeftPressed && triggerRightPressed && position.Y > 0)
                position.Y -= speed;
            else if (keyState.IsKeyDown(Keys.Down) || leftShoulderPressed && position.Y < 650)
                position.Y += speed;
            else if (keyState.IsKeyDown(Keys.Left) || triggerLeftPressed && !triggerRightPressed && position.X > 0)
                position.X -= speed;
            else if (keyState.IsKeyDown(Keys.Right) || !triggerLeftPressed && triggerRightPressed && position.X < 1150)
                position.X += speed;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Add drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(star, position, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
