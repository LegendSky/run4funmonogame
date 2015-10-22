using EV3MessengerLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Run4FunMonogame.Sprites;
using System;
using System.Collections.Generic;

namespace Run4FunMonogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Run4FunGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private KeyboardState keyState;
        private const int tileSpeed = 10;

        // private Sprite sprite = new Player();

        private const string EV3_SERIAL_PORT = "COM11";

        private int playerWidth;
        private int playerHeight;

        private int currentTile = TILE_3;
        private const int TILE_1 = 1;
        private const int TILE_2 = 2;
        private const int TILE_3 = 3;
        private const int TILE_4 = 4;
        private const int TILE_5 = 5;

        private int screenWidth;
        private int screenHeight;

        private const int tileWidth = 230;
        private const int tileHeight = 500;
        private int middleTileX;

        private int score = 0;

        SpriteFont font;

        private Player player;

        private List<Tile> tiles = new List<Tile>();

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

            player = new Player(Content.Load<Texture2D>("player"), new Vector2((screenWidth / 2) - (playerWidth / 2), screenHeight - 200));

            /*Texture2D imageTile = Content.Load<Texture2D>("bigtile");
            for (int i = 0; i < 10; i++)
            {
                Vector2 vector = generateTilePosition();
                tiles.Add(new Tile(imageTile, vector));
            }*/

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

            font = Content.Load<SpriteFont>("font");
            // TODO: use this.Content to load your game content here
            //player = Content.Load<Texture2D>("player");
            //smallTile = Content.Load<Texture2D>("smalltile");
            //bigTile = Content.Load<Texture2D>("bigtile");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            //player.Dispose();

            // EV3: Disconnect
            if (ev3Messenger.IsConnected)
                ev3Messenger.Disconnect();
        }

        private bool playerAndTileCollide(Player player, Tile tile)
        {
            Rectangle playerRect =
                new Rectangle((int)player.position.X, (int)player.position.Y,
                player.image.Width, player.image.Height);

            Rectangle tileRect =
                new Rectangle((int)tile.position.X, (int)tile.position.Y,
                    tile.image.Width, tile.image.Height);

            return playerRect.Intersects(tileRect);
        }

        private bool leftKeyPressed = false;
        private bool rightKeyPressed = false;
        private float coolDownTime = 0;
        private float coolDownTime2 = 0;
        private int intensity = 1000;
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

            coolDownTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (coolDownTime >= intensity)
            {
                intensity -= 5;
                coolDownTime = 0;
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition()));

                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i].position.Y > 1080)
                        tiles.Remove(tiles[i]);
                }
            }

            //coolDownTime2 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //if (coolDownTime2 >= 50)
            //{
            //coolDownTime2 = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                // Check for collision.
                if (playerAndTileCollide(player, tiles[i]))
                {
                    Console.WriteLine("you lost noob");
                    Exit();
                }


                // Descend tiles.
                tiles[i].position.Y += tileSpeed;
            }
            //}

            score += 1;

            // Controller triggers.
            bool triggerLeftPressed = GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.5;
            bool triggerRightPressed = GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.5;

            bool leftArrowPressed = keyState.IsKeyDown(Keys.Left);
            bool rightArrowPressed = keyState.IsKeyDown(Keys.Right);

            if ((triggerLeftPressed || leftArrowPressed) && !leftKeyPressed && currentTile > TILE_1)
            {
                if (ev3Messenger.IsConnected)
                {
                    ev3Messenger.SendMessage("Move", "Left");

                    /*
                    EV3Message message = ev3Messenger.ReadMessage();
                    if (message != null && message.MailboxTitle == "Command")
                    {
                        if (message.ValueAsText == "Left")
                        {
                            positionPlayer.X -= playerSpeed;
                            rightKeyPressed = true;
                        }
                    }*/
                    EV3Message message = ev3Messenger.ReadMessage();
                    if (message != null && message.MailboxTitle == "currentColor")
                    {
                        Console.WriteLine(message.ValueAsNumber);
                    }
                }
                player.position.X -= tileWidth;
                currentTile -= 1;
                leftKeyPressed = true;

            }
            else if ((!triggerLeftPressed && !keyState.IsKeyDown(Keys.Left)) && leftKeyPressed)
                leftKeyPressed = false;

            if ((triggerRightPressed || rightArrowPressed) && !rightKeyPressed && currentTile < TILE_5)
            {
                if (ev3Messenger.IsConnected)
                {
                    ev3Messenger.SendMessage("Move", "Right");
                    /*                      
                                        EV3Message message = ev3Messenger.ReadMessage();
                                        if (message != null && message.MailboxTitle == "Command")
                                        {
                                            if (message.ValueAsText == "Right")
                                            {
                                                positionPlayer.X += playerSpeed;
                                                rightKeyPressed = true;
                                            }
                                        }*/
                    EV3Message message = ev3Messenger.ReadMessage();
                    if (message != null && message.MailboxTitle == "currentColor")
                    {
                        Console.WriteLine(message.ValueAsNumber);
                    }
                }
                player.position.X += tileWidth;
                currentTile += 1;
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

            spriteBatch.DrawString(font, "Score: " + score, new Vector2(20, 200), Color.Red);
            spriteBatch.DrawString(font, "Respawn-rate: " + intensity, new Vector2(20, 300), Color.Blue);
            spriteBatch.Draw(player.image, player.position, Color.White);
            foreach (Tile tile in tiles)
            {
                spriteBatch.Draw(tile.image, tile.position, Color.White);
                Console.WriteLine("draw: " + tile.position.Y);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private int lastX;
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
            Console.WriteLine("x: " + x + " y: " + y);

            return new Vector2(x, y);
        }
    }
}
