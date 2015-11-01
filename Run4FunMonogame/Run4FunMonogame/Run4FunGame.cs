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

        private Rectangle boostRectangle;
        private Texture2D boostTexture;

        private int tileSpeed = 8;

        // The current tile the player is on.
        private int currentTile;

        private int score = 0;
        private int level = 1;
        private Player player;
        private List<Tile> tiles = new List<Tile>();

        // EV3: The Program.ev3Messenger is used to communicate with the Lego EV3
        //private Program.ev3Messenger Program.ev3Messenger;

        private Random random = new Random();

        private bool boostEnabled = false; // boost/dash.
        private int boostAmount = 10;
        private int colorForBoost;
        private bool colorEventEnabled = false;
        private int colorBoostCountDown = 5;

        private int oneSecondTimer = 0, tileGenerationTimer = 0, twentySecondTimer = 0, frequencyTimer = 0, tenthSecondTimer = 0;
        private int tileGenerationFrequency = 2500; // Lower is harder.

        private int playerSpeed = 0;
        private int newPositionX;

        private bool gamePaused = false;

        public Run4FunGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";

            //this.Program.ev3Messenger = Program.ev3Messenger;

            // EV3: Create an Program.ev3Messenger object which you can use to talk to the EV3.
            //Program.ev3Messenger = new Program.ev3Messenger();

            // EV3: Connect to the EV3 serial port over Bluetooth.
            //Program.ev3Messenger.Connect(EV3_SERIAL_PORT);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            currentTile = (int)tilePlayer.TILE_3_GREEN;

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
            player = new Player(Content.Load<Texture2D>("player"), new Vector2((GameConstants.WINDOW_WIDTH / 2) - (GameConstants.playerWidth / 2), GameConstants.WINDOW_HEIGHT - 200));
            boostTexture = Content.Load<Texture2D>("boost");

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
            if (Program.ev3Messenger.IsConnected)
                Program.ev3Messenger.Disconnect();
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

            // Check if current tile is 
            if (!Program.ev3Messenger.IsConnected && colorEventEnabled && currentTileIsBoostColor())
            {
                addBoostAndScoreAndReset();
            }

            boostRectangle = new Rectangle(10, 520, boostAmount * 3, 50);

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
                if (tileGenerationFrequency > 1000)
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

        private bool currentTileIsBoostColor()
        {
            return colorForBoost == currentTile;
        }

        private void colorBoostEvent()
        {
            colorForBoost = random.Next(1, 6);

            // Boost shouldn't be on player's position.
            while (currentTileIsBoostColor())
                colorForBoost = random.Next(1, 6);

            colorEventEnabled = true;
        }

        private void readEV3MessageAndDoStuff()
        {
            if (Program.ev3Messenger.IsConnected)
            {
                EV3Message message = Program.ev3Messenger.ReadMessage();
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
                }
            }
        }

        private void addBoostAndScoreAndReset()
        {
            if (boostAmount <= 100)
            {
                boostAmount += 20;
            }
            score += 5000;
            resetColorBoost();
        }

        private void addAndRemoveTiles()
        {
            int randomNum = random.Next(1, 4);
            int repeat = 1;
            switch (randomNum)
            {
                case 1:
                    repeat = 1;
                    break;
                case 2:
                    repeat = 2;
                    break;
                case 3:
                    repeat = 3;
                    break;
            }
            for (int i = 0; i < repeat; i++)
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

            if (leftKeyOrTriggerPressed() && playerSpeed == 0 && currentTile > (int)tilePlayer.TILE_1_BLACK)
                moveLeft();
            else if (rightKeyOrTriggerPressed() && playerSpeed == 0 && currentTile < (int)tilePlayer.TILE_5_RED)
                moveRight();
            else if (aOrSpacePressed())
            {
                if (boostEnabled)
                    boostEnabled = false;
                else if (boostAmount > 0)
                    boostEnabled = true;
            }

        }

        private void moveLeftPc()
        {
            newPositionX = (int)player.position.X - GameConstants.TILE_WIDTH;
            playerSpeed -= GameConstants.playerSpeedAcceleration;
            currentTile--;
        }

        private void moveRightPc()
        {
            newPositionX = (int)player.position.X + GameConstants.TILE_WIDTH;
            playerSpeed += GameConstants.playerSpeedAcceleration;
            currentTile++;
        }

        private void checkForCollision()
        {
            if (!GameConstants.collisionEnabled || boostEnabled)
                return;

            for (int i = 0; i < tiles.Count; i++)
            {
                if (playerAndTileCollide(player, tiles[i]))
                {
                    new UsernameForm(score).ShowDialog();
                    Environment.Exit(0);
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
            if (Program.ev3Messenger.IsConnected)
                Program.ev3Messenger.SendMessage("Move", "Left");
            else
                moveLeftPc();
        }

        private void moveRight()
        {
            if (Program.ev3Messenger.IsConnected)
                Program.ev3Messenger.SendMessage("Move", "Right");
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
            TILE_1_BLACK = 1, // Black
            TILE_2_BLUE = 2, // Blue
            TILE_3_GREEN = 3, // Green
            TILE_4_YELLOW = 4, // Yellow
            TILE_5_RED = 5 // Red
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
            if (Program.ev3Messenger.IsConnected)
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
            int middleTileX = (GameConstants.WINDOW_WIDTH / 2) - (GameConstants.TILE_WIDTH / 2);
            int x;
            int y;
            switch (randomNumber)
            {
                case 0:
                    x = middleTileX - (2 * GameConstants.TILE_WIDTH);
                    break;
                case 1:
                    x = middleTileX - GameConstants.TILE_WIDTH;
                    break;
                case 2:
                    x = middleTileX;
                    break;
                case 3:
                    x = middleTileX + GameConstants.TILE_WIDTH;
                    break;
                case 4:
                    x = middleTileX + (2 * GameConstants.TILE_WIDTH);
                    break;
                default:
                    x = 0;
                    break;
            }
            y = -random.Next(GameConstants.TILE_HEIGHT, GameConstants.WINDOW_HEIGHT);
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

            spriteBatch.DrawString(font, "SCORE: ", new Vector2(1600, 300), GameConstants.colorText);
            spriteBatch.DrawString(font, score.ToString(), new Vector2(1600, 350), GameConstants.colorTextNumber);

            spriteBatch.DrawString(font, "LEVEL: ", new Vector2(1600, 500), GameConstants.colorText);
            spriteBatch.DrawString(font, level.ToString(), new Vector2(1600, 550), GameConstants.colorTextNumber);

            spriteBatch.DrawString(font, "BOOST: ", new Vector2(10, 350), GameConstants.colorText);
            spriteBatch.DrawString(hugefont, boostAmount.ToString(), new Vector2(10, 400), GameConstants.colorTextNumber);
            spriteBatch.Draw(boostTexture, boostRectangle, Color.White);
            if (boostAmount >= 100)
            {
                spriteBatch.DrawString(font, "MAX BOOST", new Vector2(10, 570), GameConstants.colorText);
            }

            // Draw tiles.
            foreach (Tile tile in tiles)
                spriteBatch.Draw(tile.image, tile.position, GameConstants.colorTile);

            spriteBatch.Draw(player.image, player.position, GameConstants.colorPlayer);

            if (colorEventEnabled)
            {
                if (Program.ev3Messenger.IsConnected)
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
