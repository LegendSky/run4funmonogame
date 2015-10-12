using EV3MessengerLib;
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
    public class Run4FunGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D player;
        private KeyboardState keyState;
        private Vector2 position;
        private int speed = 200;

        // private Sprite sprite = new Player();

        private const string EV3_SERIAL_PORT = "COM14";

        private int playerWidth;
        private int playerHeight;

        private int screenWidth;
        private int screenHeight;

        // EV3: The EV3Messenger is used to communicate with the Lego EV3
        private EV3Messenger ev3Messenger;

        public Run4FunGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";

            // EV3: Create an EV3Messenger object which you can use to talk to the EV3.
            ev3Messenger = new EV3Messenger();

            // EV3: Connect to the EV3 serial port over Bluetooth.
            ev3Messenger.Connect(EV3_SERIAL_PORT);
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

            // EV3: Disconnect
            if (ev3Messenger.IsConnected)
                ev3Messenger.Disconnect();
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

            // Controller triggers.
            bool triggerLeftPressed = GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5;
            bool triggerRightPressed = GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.5;

            bool leftArrowPressed = keyState.IsKeyDown(Keys.Left);
            bool rightArrowPressed = keyState.IsKeyDown(Keys.Right);

            if ((triggerLeftPressed || leftArrowPressed) && !leftKeyPressed)
            {
                if (ev3Messenger.IsConnected)
                    ev3Messenger.SendMessage("Move", "Left");
            
                position.X -= speed;
                leftKeyPressed = true;
            }
            else if ((!triggerLeftPressed && !keyState.IsKeyDown(Keys.Left)) && leftKeyPressed)
                leftKeyPressed = false;

            if ((triggerRightPressed || rightArrowPressed) && !rightKeyPressed)
            {
                if (ev3Messenger.IsConnected)
                    ev3Messenger.SendMessage("Move", "Right");

                position.X += speed;
                rightKeyPressed = true;
            }
            else if ((!triggerRightPressed && !rightArrowPressed) && rightKeyPressed)
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
