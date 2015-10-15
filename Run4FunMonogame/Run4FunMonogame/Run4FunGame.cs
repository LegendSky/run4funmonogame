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
        private Texture2D smallTile;
        private Texture2D bigTile;

        private Vector2 positionPlayer;
        private Vector2 positionTile;
        private Vector2 positionTileRandom;

        private KeyboardState keyState;
        private const int playerSpeed = 200;
        private const int tileSpeed = 10;

        // private Sprite sprite = new Player();

        private const string EV3_SERIAL_PORT = "COM13";

        private int playerWidth;
        private int playerHeight;

        private int screenWidth;
        private int screenHeight;

        private const int tileWidth = 230;
        private const int tileHeight = 500;
        private int middleTileX;

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

            middleTileX = (screenWidth / 2) - (tileWidth / 2);

            positionPlayer = new Vector2((screenWidth / 2) - (playerWidth / 2), screenHeight - 200);
            positionTile = new Vector2(middleTileX, -tileHeight);
            positionTileRandom = generateTilePosition();

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
            smallTile = Content.Load<Texture2D>("smalltile");
            bigTile = Content.Load<Texture2D>("bigtile");
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
                {
                    ev3Messenger.SendMessage("Move", "Left");


                    EV3Message message = ev3Messenger.ReadMessage();
                    if (message != null && message.MailboxTitle == "Command")
                    {
                        if (message.ValueAsText == "Left")
                        {
                            positionPlayer.X -= playerSpeed;
                            rightKeyPressed = true;
                        }
                    }
                }

            }
            else if ((!triggerLeftPressed && !keyState.IsKeyDown(Keys.Left)) && leftKeyPressed)
                leftKeyPressed = false;

            if ((triggerRightPressed || rightArrowPressed) && !rightKeyPressed)
            {
                if (ev3Messenger.IsConnected)
                {
                    ev3Messenger.SendMessage("Move", "Right");

                    EV3Message message = ev3Messenger.ReadMessage();
                    if (message != null && message.MailboxTitle == "Command")
                    {
                        if (message.ValueAsText == "Right")
                        {
                            positionPlayer.X += playerSpeed;
                            rightKeyPressed = true;
                        }
                    }
                }


            }











            else if ((!triggerRightPressed && !rightArrowPressed) && rightKeyPressed)
                rightKeyPressed = false;

            if (positionTile.Y > 1080)
            {
                positionTile.Y = -tileHeight;
                positionTileRandom.Y = -tileHeight;
            }
            else
            {
                positionTile.Y += tileSpeed;
                positionTileRandom.Y += tileSpeed;
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

            // Add drawing code here
            spriteBatch.Begin();

            spriteBatch.Draw(player, positionPlayer, Color.White);
            //spriteBatch.Draw(smallTile, positionTile, Color.White);
            spriteBatch.Draw(bigTile, positionTileRandom, Color.White);
            //spriteBatch.Draw(bigTile, positionTile, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Vector2 generateTilePosition()
        {
            Random random = new Random();
            int randomNumber = random.Next(5);
            int x;
            int y;
            switch (randomNumber)
            {
                case 0:
                    x = middleTileX - (2 * tileWidth);
                    break;
                case 1:
                    x = middleTileX - tileWidth;
                    break;
                case 2:
                    x = middleTileX;
                    break;
                case 3:
                    x = middleTileX + tileWidth;
                    break;
                case 4:
                    x = middleTileX + (2 * tileWidth);
                    break;
                default:
                    x = 0;
                    break;
            }
            y = -random.Next(tileHeight, screenHeight);
            return new Vector2(x, y);
        }
    }
}
