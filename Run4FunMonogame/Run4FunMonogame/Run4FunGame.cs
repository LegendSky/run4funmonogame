using EV3MessengerLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Run4Fun.Sprites;
using System;
using System.Collections.Generic;

namespace Run4Fun
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

        private int tileSpeed = 8;

        private const string EV3_SERIAL_PORT = "COM6";

        private const int playerWidth = 100, playerHeight = 100;
        private const int TILE_WIDTH = 230, TILE_HEIGHT = 500, WINDOW_WIDTH = 1920, WINDOW_HEIGHT = 1080;

        // The current tile the player is on.
        private int currentTile;

        private int score = 0;
        private Player player;
        private List<Tile> tiles = new List<Tile>();

        // EV3: The EV3Messenger is used to communicate with the Lego EV3
        private EV3Messenger ev3Messenger;

        Random random = new Random();

        private const bool collisionEnabled = true;
        private bool hyperMode = false; // hypermode, double score.

        private bool boostEnabled = false; // boost/dash.
        private int boostAmount = 0;
        private int colorForBoost;
        private bool colorEventEnabled = false;
        private int colorBoostCountDown = 5;

        private int oneSecondTimer = 0, twentySecondTimer = 0, tenthSecondTimer = 0, tileSpeedTime = 0;

        private int playerSpeed;
        private int playerSpeedAcceleration = 10; // 10 or 23
        private int newPositionX;

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
            currentTile = (int)tilePlayer.TILE_3;

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

            player = new Player(Content.Load<Texture2D>("player"), new Vector2((WINDOW_WIDTH / 2) - (playerWidth / 2), WINDOW_HEIGHT - 200));
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

            // Check for collision.
            checkForCollision();

            // Descent tiles.
            descentTiles();

            // Increase score.
            increaseScore();

            // Check for color event
            if (!ev3Messenger.IsConnected && colorEventEnabled)
                checkColorLane();

            // Every tenth second.
            tenthSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (boostEnabled)
            {
                if (boostAmount <= 0)
                    boostEnabled = false;

                if (tenthSecondTimer >= 100)
                {
                    tenthSecondTimer = 0;
                    boostAmount--;
                }
            }

            // Every second.
            oneSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (oneSecondTimer >= 1000)
            {
                oneSecondTimer = 0;
                addAndRemoveTiles();

                if (colorEventEnabled)
                {
                    if (colorBoostCountDown <= 0)
                        resetColorBoost();
                    else
                        colorBoostCountDown--;
                }
            }

            // Every 20 seconds.
            twentySecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (twentySecondTimer >= 20000)
            {
                twentySecondTimer = 0;
                colorBoostEvent();

                tileSpeedTime = 0;
                tileSpeed++;
            }

            if (player.position.X == newPositionX)
                playerSpeed = 0;
            player.position.X += playerSpeed;

            base.Update(gameTime);
        }

        private void resetColorBoost()
        {
            colorEventEnabled = false;
            colorBoostCountDown = 5;
            colorForBoost = 0;
        }

        private void checkColorLane()
        {
            if (colorForBoost == currentTile)
                addBoostAndScoreAndReset();
        }

        private void colorBoostEvent()
        {
            Random random = new Random();
            colorForBoost = random.Next(1, 6);

            // Boost shouldn't be on player's position.
            while (colorForBoost == player.position.X)
                colorForBoost = random.Next(1, 6);

            colorEventEnabled = true;
        }

        private void readEV3MessageAndDoStuff()
        {
            if (ev3Messenger.IsConnected)
            {
                EV3Message message = ev3Messenger.ReadMessage();
                if (message != null && message.MailboxTitle == "Command")
                {
                    if (message.ValueAsText == "Left")
                        moveLeftPc();
                    else if (message.ValueAsText == "Right")
                        moveRightPc();
                }
                else if (message != null && message.MailboxTitle == "Color")
                {
                    if (colorEventEnabled && colorForBoost == message.ValueAsNumber) //TODO: make it give points only once!
                        addBoostAndScoreAndReset();
                    /*
                    switch ((int)message.ValueAsNumber)
                    {
                        case 1:
                            if (colorForBoost == 1)
                                boostEnabled = true;
                            Console.WriteLine("color 1: black");
                            break;
                        case 2:
                            if (colorForBoost == 1)
                                boostEnabled = true;
                            Console.WriteLine("color 2: blue");
                            break;
                        case 3:
                            if (colorForBoost == 1)
                                boostEnabled = true;
                            Console.WriteLine("color 3: green");
                            break;
                        case 4:
                            if (colorForBoost == 1)
                                boostEnabled = true;
                            Console.WriteLine("color 4: yellow");
                            break;
                        case 5:
                            if (colorForBoost == 1)
                                boostEnabled = true;
                            Console.WriteLine("color 5: red");
                            break;
                    }*/
                }
            }
        }

        private void addBoostAndScoreAndReset()
        {
            boostAmount += 20;
            score += 5000;
            resetColorBoost();
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

            if (leftKeyOrTriggerPressed() && playerSpeed == 0 && currentTile > (int)tilePlayer.TILE_1)
                moveLeft();
            else if (rightKeyOrTriggerPressed() && playerSpeed == 0 && currentTile < (int)tilePlayer.TILE_5)
                moveRight();
            else if (aOrSpacePressed())
            {
                if (boostEnabled)
                    boostEnabled = false;
                else if (boostAmount > 0)
                    boostEnabled = true;
            }

            // Exit button.
            else if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();
        }

        private void moveLeftPc()
        {
            newPositionX = (int)player.position.X - TILE_WIDTH;
            playerSpeed -= playerSpeedAcceleration;
            currentTile--;
        }

        private void moveRightPc()
        {
            newPositionX = (int)player.position.X + TILE_WIDTH;
            playerSpeed += playerSpeedAcceleration;
            currentTile++;
        }

        private void checkForCollision()
        {
            if (!collisionEnabled || boostEnabled)
                return;

            for (int i = 0; i < tiles.Count; i++)
            {
                if (playerAndTileCollide(player, tiles[i]))
                    Exit();
            }
        }

        private void descentTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].position.Y += boostEnabled ? tileSpeed * 5 : tileSpeed;
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

        private enum tilePlayer
        {
            TILE_1 = 1, // Black
            TILE_2 = 2, // Blue
            TILE_3 = 3, // Green
            TILE_4 = 4, // Yellow
            TILE_5 = 5 // Red
        }

        private bool playerAndTileCollide(Player player, Tile tile)
        {
            Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.image.Width, player.image.Height);
            Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, tile.image.Width, tile.image.Height);

            return playerRect.Intersects(tileRect);
        }

        private void increaseScore()
        {
            score++;
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

            drawTwoTextsAt20XAnd260X("Score: ", score.ToString(), 200);
            drawTwoTextsAt20XAnd260X("Tile speed: ", tileSpeed.ToString(), 250);
            drawTwoTextsAt20XAnd260X("Hypermode: ", hyperMode ? "On" : "Off", 300);
            drawTwoTextsAt20XAnd260X("Boost: ", boostEnabled ? "On" : "Off", 350);
            drawTwoTextsAt20XAnd260X("Boost amount: ", boostAmount.ToString(), 400);

            drawTwoTextsAt20XAnd260X("Color event: ", colorEventEnabled ? "On" : "Off", 500);
            drawTwoTextsAt20XAnd260X("Color for boost: ", convertColorNumberToString(colorForBoost), 550);

            drawBlackTextAt20X("Time left", 650);
            drawTwoTextsAt20XAnd260X("to take boost: ", colorEventEnabled ? colorBoostCountDown.ToString() + "s" : "-", 700);

            spriteBatch.Draw(player.image, player.position, Color.White);

            // Draw tiles.
            foreach (Tile tile in tiles)
                spriteBatch.Draw(tile.image, tile.position, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private string convertColorNumberToString(int colorNumber)
        {
            string color = "-";
            if (ev3Messenger.IsConnected)
            {
                switch (colorNumber)
                {
                    case 1:
                        color = "Black";
                        break;
                    case 2:
                        color = "Blue";
                        break;
                    case 3:
                        color = "Green";
                        break;
                    case 4:
                        color = "Yellow";
                        break;
                    case 5:
                        color = "Red";
                        break;
                }
            }
            else
            {
                switch (colorNumber)
                {
                    case 1:
                        color = "Black(1)";
                        break;
                    case 2:
                        color = "Blue(2)";
                        break;
                    case 3:
                        color = "Green(3)";
                        break;
                    case 4:
                        color = "Yellow(4)";
                        break;
                    case 5:
                        color = "Red(5)";
                        break;

                }
            }
            return color;
        }

        private void drawBlackTextAt20X(string text, int y)
        {
            spriteBatch.DrawString(font, text, new Vector2(10, y), Color.Black);
        }

        private void drawTwoTextsAt20XAnd260X(string text1, string text2, int y)
        {
            drawBlackTextAt20X(text1, y);
            spriteBatch.DrawString(font, text2, new Vector2(260, y), Color.Red);
        }

        /// <summary>
        /// Generates random position for tiles.
        /// </summary>
        /// <returns></returns>
        private Vector2 generateTilePosition()
        {
            int randomNumber = random.Next(5);
            int middleTileX = (WINDOW_WIDTH / 2) - (TILE_WIDTH / 2);
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
            y = -random.Next(TILE_HEIGHT, WINDOW_HEIGHT);
            //Console.WriteLine("x: " + x + " y: " + y);

            return new Vector2(x, y);
        }
    }
}
