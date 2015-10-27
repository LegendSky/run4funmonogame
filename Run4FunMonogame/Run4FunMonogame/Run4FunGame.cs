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
    /// Run4Fun game by A4B 2015.
    /// </summary>
    public class Run4FunGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private KeyboardState prevKeyState, keyState;
        private GamePadState prevGamePadState, gamePadState;

        private int tileSpeed = 10;

        private const string EV3_SERIAL_PORT = "COM6";

        private const int playerWidth = 100, playerHeight = 100;
        private const int TILE_WIDTH = 230, TILE_HEIGHT = 500;

        // The current tile the pc is on.
        private int currentTile;
        // The current tile the EV3 is on.
        private int currentTileEV3;
        private int currentColor = (int)tileEV3.EV3_TILE_3;

        private int score = 0;
        private Player player;
        private List<Tile> tiles = new List<Tile>();

        // EV3: The EV3Messenger is used to communicate with the Lego EV3
        private EV3Messenger ev3Messenger;

        private const bool collisionEnabled = true;
        private bool boost = false; // boost/dash.
        private bool hyperMode = false; // hypermode, double score.

        private float spawnTime = 0;
        private int intensity = 1000;
        private int playerSpeed;
        private int playerSpeedAcceleration = 10; // 10 or 23
        private float newPositionX;

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
            currentTile = (int)tilePc.TILE_3;
            currentTileEV3 = (int)tileEV3.EV3_TILE_3;

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

            player = new Player(Content.Load<Texture2D>("player"), new Vector2((Window.ClientBounds.Width / 2) - (playerWidth / 2), Window.ClientBounds.Height - 200));
            font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // EV3 Disconnect
            if (ev3Messenger.IsConnected)
                ev3Messenger.Disconnect();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handles key/button presses.
            handleKeys();

            // Reads incoming messages and does the appropiate action. 
            readEV3MessageAndDoStuff();

            // Check for collision and descent tiles.
            checkForCollisionAndDescentTiles();

            // Increase score.
            increaseScore();

            // Add and remove tiles.
            spawnTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spawnTime >= intensity)
            {
                //intensity -= 5;
                //tileSpeed += 1;
                spawnTime = 0;
                addAndRemoveTiles();
            }

            if (player.position.X == newPositionX)
                playerSpeed = 0;
            player.position.X += playerSpeed;

            base.Update(gameTime);
        }

        private void readEV3MessageAndDoStuff()
        {
            if (ev3Messenger.IsConnected)
            {
                EV3Message message = ev3Messenger.ReadMessage();
                if (message != null && message.MailboxTitle == "Command")
                {
                    if (message.ValueAsText == "Left")
                    {
                        moveLeftPc();
                    }
                    else if (message.ValueAsText == "Right")
                    {
                        moveRightPc();
                    }
                }
                else if (message != null && message.MailboxTitle == "Color")
                {
                    switch ((int)message.ValueAsNumber)
                    {
                        case 1:
                            Console.WriteLine("color 1: black");
                            break;
                        case 2:
                            Console.WriteLine("color 2: blue");
                            break;
                        case 3:
                            Console.WriteLine("color 3: green");
                            break;
                        case 4:
                            Console.WriteLine("color 4: yellow");
                            break;
                        case 5:
                            Console.WriteLine("color 5: red");
                            break;
                    }
                }
            }
        }

        private void addAndRemoveTiles()
        {
            tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition()));
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].position.Y > 1080)
                    tiles.Remove(tiles[i]);
            }
        }

        private void handleKeys()
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            prevGamePadState = gamePadState;
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (leftKeyOrTriggerPressed() && playerSpeed == 0 && currentTile > (int)tilePc.TILE_1)
                moveLeft();
            else if (rightKeyOrTriggerPressed() && playerSpeed == 0 && currentTile < (int)tilePc.TILE_5)
                moveRight();
            else if (aOrSpacePressed())
                boost = !boost;

            // Exit button.
            else if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();
        }

        private void moveLeftPc()
        {
            newPositionX = player.position.X - TILE_WIDTH;
            playerSpeed -= playerSpeedAcceleration;
            currentTile--;
        }

        private void moveRightPc()
        {
            newPositionX = player.position.X + TILE_WIDTH;
            playerSpeed += playerSpeedAcceleration;
            currentTile++;
        }

        private void checkForCollisionAndDescentTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                // Check for collision.
                if (collisionEnabled && playerAndTileCollide(player, tiles[i]))
                    Exit();

                // Descend tiles.
                tiles[i].position.Y += boost ? tileSpeed * 5 : tileSpeed;
            }
        }

        private void moveLeft()
        {
            if (ev3Messenger.IsConnected)
                ev3Messenger.SendMessage("Move", "Left");
            else
                moveLeftPc();
        }

        private void moveRight()
        {
            if (ev3Messenger.IsConnected)
                ev3Messenger.SendMessage("Move", "Right");
            else
                moveRightPc();
        }

        private bool leftKeyOrTriggerPressed()
        {
            return ((gamePadState.Triggers.Left >= 0.5 && prevGamePadState.Triggers.Left < 0.5)
                || (keyState.IsKeyDown(Keys.Left) && prevKeyState.IsKeyUp(Keys.Left)));
        }

        private bool rightKeyOrTriggerPressed()
        {
            return ((gamePadState.Triggers.Right >= 0.5 && prevGamePadState.Triggers.Right < 0.5)
                || (keyState.IsKeyDown(Keys.Right) && prevKeyState.IsKeyUp(Keys.Right)));
        }

        private bool aOrSpacePressed()
        {
            return ((gamePadState.Buttons.A == ButtonState.Pressed && prevGamePadState.Buttons.A != ButtonState.Pressed)
                || (keyState.IsKeyDown(Keys.Space) && prevKeyState.IsKeyUp(Keys.Space)));
        }

        private enum tilePc
        {
            TILE_1 = 1,
            TILE_2 = 2,
            TILE_3 = 3,
            TILE_4 = 4,
            TILE_5 = 5
        }

        private enum tileEV3
        {
            EV3_TILE_1 = 1,// black
            EV3_TILE_2 = 2,// blue
            EV3_TILE_3 = 3,// green
            EV3_TILE_4 = 4,// yellow
            EV3_TILE_5 = 5// red
        }

        private bool playerAndTileCollide(Player player, Tile tile)
        {
            Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.image.Width, player.image.Height);
            Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, tile.image.Width, tile.image.Height);

            return playerRect.Intersects(tileRect);
        }

        private void increaseScore()
        {
            score += 1;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.Gold);
            
            spriteBatch.Begin();

            int text1X = 20;
            int text2X = 280;

            Color color1 = Color.Black;
            Color color2 = Color.Red;

            spriteBatch.DrawString(font, "Score: ", new Vector2(text1X, 200), color1);
            spriteBatch.DrawString(font, score.ToString(), new Vector2(text2X, 200), color2);

            spriteBatch.DrawString(font, "Tile speed: ", new Vector2(text1X, 250), color1);
            spriteBatch.DrawString(font, tileSpeed.ToString(), new Vector2(text2X, 250), color2);

            spriteBatch.DrawString(font, "Boost: ", new Vector2(text1X, 300), color1);
            spriteBatch.DrawString(font, boost ? "On" : "Off", new Vector2(text2X, 300), color2);

            spriteBatch.DrawString(font, "Hypermode: ", new Vector2(text1X, 350), color1);
            spriteBatch.DrawString(font, hyperMode ? "On" : "Off", new Vector2(text2X, 350), color2);

            spriteBatch.Draw(player.image, player.position, Color.White);

            // Draw tiles.
            foreach (Tile tile in tiles)
                spriteBatch.Draw(tile.image, tile.position, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Generates random position for tiles.
        /// </summary>
        /// <returns></returns>
        private Vector2 generateTilePosition()
        {
            Random random = new Random();
            int randomNumber = random.Next(5);
            int middleTileX = (Window.ClientBounds.Width / 2) - (TILE_WIDTH / 2);
            int x;
            int y;
            switch (randomNumber)
            {
                case 0:
                    x = middleTileX - (2 * TILE_WIDTH);
                    break;
                case 1:
                    x = middleTileX - TILE_WIDTH;
                    break;
                case 2:
                    x = middleTileX;
                    break;
                case 3:
                    x = middleTileX + TILE_WIDTH;
                    break;
                case 4:
                    x = middleTileX + (2 * TILE_WIDTH);
                    break;
                default:
                    x = 0;
                    break;
            }
            y = -random.Next(TILE_HEIGHT, Window.ClientBounds.Height);
            //Console.WriteLine("x: " + x + " y: " + y);

            return new Vector2(x, y);
        }
    }
}
