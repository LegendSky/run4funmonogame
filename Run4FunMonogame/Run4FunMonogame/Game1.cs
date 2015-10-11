using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Run4FunMonogame.Sprites;
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
        private Texture2D player;
        private KeyboardState keyState;
        private Vector2 position;
        private int speed = 200;

        private Sprite sprite = new Player();

        private int playerWidth;
        private int playerHeight;

        private int screenWidth;
        private int screenHeight;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.AllowUserResizing = true;

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
            keyState = Keyboard.GetState();

            screenWidth = Window.ClientBounds.Width;
            screenHeight = Window.ClientBounds.Height;

            playerWidth = 100;
            playerHeight = 100;

            position = new Vector2((screenWidth / 2) - (playerWidth / 2), screenHeight - 200);

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
            player = Content.Load<Texture2D>("player");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            player.Dispose();
        }

        bool leftKeyPressed = false;
        bool rightKeyPressed = false;

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

            // Controller buttons.
            bool triggerLeftPressed = GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5;
            bool triggerRightPressed = GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.5;
            bool leftShoulderPressed = GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed;

            if ((triggerLeftPressed || keyState.IsKeyDown(Keys.Left)) && !leftKeyPressed)
            {
                position.X -= speed;
                leftKeyPressed = true;
            }
            else if ((!triggerLeftPressed && !keyState.IsKeyDown(Keys.Left)) && leftKeyPressed)
                leftKeyPressed = false;

            if ((triggerRightPressed || keyState.IsKeyDown(Keys.Right)) && !rightKeyPressed)
            {
                position.X += speed;
                rightKeyPressed = true;
            }
            else if ((!triggerRightPressed && !keyState.IsKeyDown(Keys.Right)) && rightKeyPressed)
                rightKeyPressed = false;

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
            spriteBatch.Draw(player, position, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
