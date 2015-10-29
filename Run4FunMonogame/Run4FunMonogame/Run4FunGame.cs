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

        private SpriteFont smallfont;
        private SpriteFont font;
        private SpriteFont bigfont;
        private SpriteFont hugefont;

        private KeyboardState prevKeyState, keyState;
        private GamePadState prevGamePadState, gamePadState;
        private Texture2D backgroundImage;

        private int tileSpeed = 8;

        private const string EV3_SERIAL_PORT = "COM6";

        private const int playerWidth = 100, playerHeight = 100;
        private const int TILE_WIDTH = 230, TILE_HEIGHT = 500, WINDOW_WIDTH = 1920, WINDOW_HEIGHT = 1080;

        // The current tile the player is on.
        private int currentTile;

        private int score = 0;
        private int level = 1;
        private Player player;
        private List<Tile> tiles = new List<Tile>();

        // EV3: The EV3Messenger is used to communicate with the Lego EV3
        private EV3Messenger ev3Messenger;

        private Random random = new Random();

        private const bool collisionEnabled = true;
        private bool hyperMode = false; // hypermode, double score.

        private bool boostEnabled = false; // boost/dash.
        private int boostAmount = 10;
        private int colorForBoost;
        private bool colorEventEnabled = false;
        private int colorBoostCountDown = 5;

        private int oneSecondTimer = 0, tileGenerationTimer = 0, twentySecondTimer = 0, frequencyTimer = 0, tenthSecondTimer = 0;
        private int tileGenerationFrequency = 1500; // Lower is harder.

        private int playerSpeed;
        private int playerSpeedAcceleration = 10; // 10 or 23
        private int newPositionX;

        private Color colorText = Color.Black;
        private Color colorTextNumber = Color.Black;
        private Color colorTile = Color.Gray;
        private Color colorPlayer = Color.Red;

        private bool gamePaused = false;

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

            backgroundImage = Content.Load<Texture2D>("background");
            player = new Player(Content.Load<Texture2D>("player"), new Vector2((WINDOW_WIDTH / 2) - (playerWidth / 2), WINDOW_HEIGHT - 200));

            smallfont = Content.Load<SpriteFont>("smallfont");
            font = Content.Load<SpriteFont>("font");
            bigfont = Content.Load<SpriteFont>("bigfont");
            hugefont = Content.Load<SpriteFont>("hugefont");
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

            if (gamePaused)
            {
                return;
            }

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
                if (colorEventEnabled)
                {
                    if (colorBoostCountDown <= 0)
                        resetColorBoost();
                    else
                        colorBoostCountDown--;
                }
            }

            // Starts at every 1.5 seconds.
            tileGenerationTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (tileGenerationTimer >= tileGenerationFrequency)
            {
                tileGenerationTimer = 0;
                addAndRemoveTiles();
            }

            // Frequency increaser
            frequencyTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frequencyTimer >= 30000)
            {
                frequencyTimer = 0;
                if (tileGenerationFrequency > 500)
                    tileGenerationFrequency -= 100; // lower is harder
            }

            // Every 20 seconds.
            twentySecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (twentySecondTimer >= 20000)
            {
                twentySecondTimer = 0;
                colorBoostEvent();

                nextLevel();
            }

            if (player.position.X == newPositionX)
                playerSpeed = 0;
            player.position.X += playerSpeed;

            base.Update(gameTime);
        }

        private void nextLevel()
        {
            level++;
            tileSpeed++;
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

            if (escOrPOrBackOrStartPressed())
                gamePaused = !gamePaused;
            if (gamePaused)
                return;

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

        }

        /*private void pause()
        {
            gamePaused = true;
        }

        private void resume()
        {
            gamePaused = false;
        }*/

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
                {
                    new UsernameForm(score).ShowDialog();
                    Exit();
                }
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

        private bool escOrPOrBackOrStartPressed()
        {
            return ((gamePadState.Buttons.Start == ButtonState.Pressed && prevGamePadState.Buttons.Start != ButtonState.Pressed)
                || (gamePadState.Buttons.Back == ButtonState.Pressed && prevGamePadState.Buttons.Back != ButtonState.Pressed)
                || (keyState.IsKeyDown(Keys.Escape) && prevKeyState.IsKeyUp(Keys.Escape))
                || (keyState.IsKeyDown(Keys.P) && prevKeyState.IsKeyUp(Keys.P)));
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

        private Color colorEventColor()
        {
            Color color;
            switch (colorForBoost)
            {
                case 1:
                    color = Color.Black;
                    break;
                case 2:
                    color = Color.Blue;
                    break;
                case 3:
                    color = Color.Green;
                    break;
                case 4:
                    color = Color.Yellow;
                    break;
                case 5:
                    color = Color.Red;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            return color;
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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(backgroundImage, new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height), Color.White);

            spriteBatch.DrawString(font, "SCORE: ", new Vector2(1600, 300), colorText);
            spriteBatch.DrawString(font, score.ToString(), new Vector2(1600, 350), colorTextNumber);

            spriteBatch.DrawString(font, "LEVEL: ", new Vector2(1600, 500), colorText);
            spriteBatch.DrawString(font, level.ToString(), new Vector2(1600, 550), colorTextNumber);

            spriteBatch.DrawString(font, "BOOST: ", new Vector2(10, 350), colorText);
            spriteBatch.DrawString(hugefont, boostAmount.ToString(), new Vector2(10, 400), colorTextNumber);

            spriteBatch.Draw(player.image, player.position, colorPlayer);

            // Draw tiles.
            foreach (Tile tile in tiles)
                spriteBatch.Draw(tile.image, tile.position, colorTile);

            if (colorEventEnabled)
            {
                if (ev3Messenger.IsConnected)
                    spriteBatch.DrawString(bigfont, "BOOST ON " + convertColorNumberToString(colorForBoost).ToUpper() + " " + colorBoostCountDown.ToString() + "s", new Vector2(500, 200), colorEventColor());
                else
                    spriteBatch.DrawString(bigfont, "BOOST IN LANE (" + colorForBoost + ") " + colorBoostCountDown.ToString() + "s", new Vector2(500, 200), colorEventColor());
            }

            if (gamePaused)
                spriteBatch.DrawString(bigfont, "GAME PAUSED", new Vector2(700, 400), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            new StartForm().ShowDialog();
        }
    }
}
