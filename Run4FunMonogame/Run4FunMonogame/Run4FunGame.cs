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
        private int currentLane;

        private int score = 0;
        private int level = 1;
        private Player player;
        private List<Tile> tiles = new List<Tile>();

        private Random random = new Random();

        private bool boostEnabled = false;
        private int boostAmount = 10;
        private int colorForBoost;
        private bool colorEventEnabled = false;
        private int colorBoostCountDown = 5;

        private int oneSecondTimer = 0, tileGenerationTimer = 0, twentySecondTimer = 0, frequencyTimer = 0, tenthSecondTimer = 0;

        // Milliseconds before spawning next wave.
        private int tileGenerationFrequency = 2500;

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
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            currentLane = (int)lanePlayer.LANE_3_GREEN;
            updateBoostBar();
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

            // Load images.
            backgroundImage = Content.Load<Texture2D>("background");
            player = new Player(Content.Load<Texture2D>("player"), new Vector2((GameConstants.WINDOW_WIDTH / 2) - (GameConstants.playerWidth / 2), GameConstants.WINDOW_HEIGHT - 200));
            boostTexture = Content.Load<Texture2D>("boost");

            // Load fonts.
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

            // Stop updating the game if paused.
            if (gamePaused)
            {
                return;
            }

            // Reads incoming messages and does the appropiate action. 
            readEV3MessageAndDoAction();

            // Check for collision.
            checkForCollision();

            // Descent tiles.
            descentTiles();

            // Increase score.
            increaseScore();

            // Check if current tile is 
            if (!Program.ev3Messenger.IsConnected && colorEventEnabled && currentLaneIsBoostColor())
            {
                addBoostAndScoreAndUpdate();
            }

            // Every tenth second.
            tenthSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (boostEnabled)
            {
                if (boostAmount <= 0)
                {
                    boostEnabled = false;
                }

                if (tenthSecondTimer >= 100)
                {
                    tenthSecondTimer = 0;
                    boostAmount--;
                    updateBoostBar();
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
                    {
                        resetColorEvent();
                    }
                    else
                    {
                        colorBoostCountDown--;
                    }
                }
            }

            // Spawn and remove tiles.
            tileGenerationTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (tileGenerationTimer >= tileGenerationFrequency)
            {
                tileGenerationTimer = 0;
                spawnAndRemoveTiles();
            }

            // Increase tile spawn frequency.
            frequencyTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frequencyTimer >= 30000)
            {
                frequencyTimer = 0;
                if (tileGenerationFrequency > 1000)
                {
                    tileGenerationFrequency -= 100; // lower is harder
                }
            }

            // Every 20 seconds.
            twentySecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (twentySecondTimer >= 20000)
            {
                twentySecondTimer = 0;
                startColorBoostEvent();

                nextLevel();
            }

            if (player.position.X == newPositionX)
            {
                playerSpeed = 0;
            }
            player.position.X += playerSpeed;

            base.Update(gameTime);
        }

        /// <summary>
        /// Update the boost bar.
        /// </summary>
        private void updateBoostBar()
        {
            boostRectangle = new Rectangle(10, 520, boostAmount * 3, 50);
        }

        /// <summary>
        /// Increase level and tilespeed.
        /// </summary>
        private void nextLevel()
        {
            level++;
            tileSpeed++;
        }

        /// <summary>
        /// Reset color event.
        /// </summary>
        private void resetColorEvent()
        {
            colorEventEnabled = false;
            colorBoostCountDown = 5;
            colorForBoost = 0;
        }

        /// <summary>
        /// Checkes whether the color is in the current lane.
        /// </summary>
        /// <returns></returns>
        private bool currentLaneIsBoostColor()
        {
            return colorForBoost == currentLane;
        }

        /// <summary>
        /// Start color boost event.
        /// </summary>
        private void startColorBoostEvent()
        {
            colorForBoost = random.Next(1, 6);

            // Boost shouldn't be on player's position.
            while (currentLaneIsBoostColor())
            {
                colorForBoost = random.Next(1, 6);
            }

            colorEventEnabled = true;
        }

        /// <summary>
        /// Actions when EV3 messages are received.
        /// </summary>
        private void readEV3MessageAndDoAction()
        {
            if (Program.ev3Messenger.IsConnected)
            {
                EV3Message message = Program.ev3Messenger.ReadMessage();
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
                    if (colorEventEnabled && colorForBoost == message.ValueAsNumber)
                    {
                        addBoostAndScoreAndUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Increase boost and score amount, reset the color event and update boost bar.
        /// </summary>
        private void addBoostAndScoreAndUpdate()
        {
            if (boostAmount <= 100)
            {
                boostAmount += 20;
            }

            score += 5000;
            resetColorEvent();
            updateBoostBar();
        }

        /// <summary>
        /// Spawn new tiles and remove tiles out of vision.
        /// </summary>
        private void spawnAndRemoveTiles()
        {
            // 1 to 3 tiles will spawn on random lanes.
            int repeat = random.Next(1, 4);
            for (int i = 0; i < repeat; i++)
            {
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition(false)));
            }

            // 50% chance to spawn tile on the player's lane.
            if (random.Next(1, 3) == 1)
            {
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition(true)));
            }

            // Remove out of vision tiles.
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].position.Y > 1080)
                    tiles.Remove(tiles[i]);
            }
        }

        /// <summary>
        /// Actions for key presses.
        /// </summary>
        private void handleKeys()
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            prevGamePadState = gamePadState;
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (pauseButtonPressed())
            {
                gamePaused = !gamePaused;
            }

            if (gamePaused)
            {
                return;
            }

            if (leftArrowOrTriggerPressed() && playerSpeed == 0 && currentLane > (int)lanePlayer.LANE_1_BLACK)
            {
                if (Program.ev3Messenger.IsConnected)
                {
                    moveLeftEV3();
                }
                else
                {
                    moveLeftPc();
                }
            }

            else if (rightArrowOrTriggerPressed() && playerSpeed == 0 && currentLane < (int)lanePlayer.LANE_5_RED)
            {
                if (Program.ev3Messenger.IsConnected)
                {
                    moveRightEV3();
                }
                else
                {
                    moveRightPc();
                }
            }

            else if (boostPressed())
            {
                if (boostEnabled)
                {
                    boostEnabled = false;
                }
                else if (boostAmount > 0)
                {
                    boostEnabled = true;
                }
            }

        }

        /// <summary>
        /// Move the player ingame left.
        /// </summary>
        private void moveLeftPc()
        {
            newPositionX = (int)player.position.X - GameConstants.TILE_WIDTH;
            playerSpeed -= GameConstants.playerSpeedAcceleration;
            currentLane--;
        }

        /// <summary>
        /// Move the player ingame right.
        /// </summary>
        private void moveRightPc()
        {
            newPositionX = (int)player.position.X + GameConstants.TILE_WIDTH;
            playerSpeed += GameConstants.playerSpeedAcceleration;
            currentLane++;
        }

        /// <summary>
        /// Check if the player rectangle touches any tile rectangles.
        /// </summary>
        private void checkForCollision()
        {
            if (!GameConstants.collisionEnabled || boostEnabled)
            {
                return;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                if (playerAndTileCollide(player, tiles[i]))
                {
                    new UsernameForm(score).ShowDialog();
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Descent all tiles, speed depends on tileSpeed.
        /// </summary>
        private void descentTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].position.Y += boostEnabled ? tileSpeed * 5 : tileSpeed;
            }
        }

        /// <summary>
        /// Send message to EV3 with title "Move" and message "Left".
        /// </summary>
        private void moveLeftEV3()
        {
            Program.ev3Messenger.SendMessage("Move", "Left");
        }

        /// <summary>
        /// Send message to EV3 with title "Move" and message "Right".
        /// </summary>
        private void moveRightEV3()
        {
            Program.ev3Messenger.SendMessage("Move", "Right");
        }

        /// <summary>
        /// Checks whether left arrow(PC) or trigger(controller) is pressed.
        /// </summary>
        /// <returns></returns>
        private bool leftArrowOrTriggerPressed()
        {
            return ((gamePadState.Triggers.Left >= 0.5 && prevGamePadState.Triggers.Left < 0.5)
                || (keyState.IsKeyDown(Keys.Left) && prevKeyState.IsKeyUp(Keys.Left)));
        }

        /// <summary>
        /// Checks whether right arrow(PC) or trigger(controller) is pressed.
        /// </summary>
        /// <returns></returns>
        private bool rightArrowOrTriggerPressed()
        {
            return ((gamePadState.Triggers.Right >= 0.5 && prevGamePadState.Triggers.Right < 0.5)
                || (keyState.IsKeyDown(Keys.Right) && prevKeyState.IsKeyUp(Keys.Right)));
        }

        /// <summary>
        /// Checks whether A or Space is pressed.
        /// </summary>
        /// <returns></returns>
        private bool boostPressed()
        {
            return ((gamePadState.Buttons.A == ButtonState.Pressed && prevGamePadState.Buttons.A != ButtonState.Pressed)
                || (keyState.IsKeyDown(Keys.Space) && prevKeyState.IsKeyUp(Keys.Space)));
        }

        /// <summary>
        /// Checks whether either Start, Back, Escape or P is pressed.
        /// </summary>
        /// <returns></returns>
        private bool pauseButtonPressed()
        {
            return ((gamePadState.Buttons.Start == ButtonState.Pressed && prevGamePadState.Buttons.Start != ButtonState.Pressed)
                || (gamePadState.Buttons.Back == ButtonState.Pressed && prevGamePadState.Buttons.Back != ButtonState.Pressed)
                || (keyState.IsKeyDown(Keys.Escape) && prevKeyState.IsKeyUp(Keys.Escape))
                || (keyState.IsKeyDown(Keys.P) && prevKeyState.IsKeyUp(Keys.P)));
        }

        /// <summary>
        /// 
        /// </summary>
        private enum lanePlayer
        {
            LANE_1_BLACK = 1, // Black
            LANE_2_BLUE = 2, // Blue
            LANE_3_GREEN = 3, // Green
            LANE_4_YELLOW = 4, // Yellow
            LANE_5_RED = 5 // Red
        }

        /// <summary>
        /// Checks whether the rectangle of player touches the rectangle of any tile.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        private bool playerAndTileCollide(Player player, Tile tile)
        {
            Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.image.Width, player.image.Height);
            Rectangle tileRect = new Rectangle((int)tile.position.X, (int)tile.position.Y, tile.image.Width, tile.image.Height);

            return playerRect.Intersects(tileRect);
        }

        /// <summary>
        /// Increase score.
        /// </summary>
        private void increaseScore()
        {
            score++;
        }

        /// <summary>
        /// The color for boost event.
        /// </summary>
        /// <returns></returns>
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
                default:
                    color = Color.Green;
                    break;
                case 4:
                    color = Color.Yellow;
                    break;
                case 5:
                    color = Color.Red;
                    break;
            }
            return color;
        }

        /// <summary>
        /// Convert a color number to the name of the color.
        /// </summary>
        /// <param name="colorNumber"></param>
        /// <returns></returns>
        private string getColorNameByNumber(int colorNumber)
        {
            string color = "";
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
        /// Get X-coordinate for lane.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private int getTileXCoord(int tile)
        {
            int middleTileX = (GameConstants.WINDOW_WIDTH / 2) - (GameConstants.TILE_WIDTH / 2);
            int x;
            switch (tile)
            {
                case 1:
                    x = middleTileX - (2 * GameConstants.TILE_WIDTH);
                    break;
                case 2:
                    x = middleTileX - (GameConstants.TILE_WIDTH);
                    break;
                case 3:
                default:
                    x = middleTileX;
                    break;
                case 4:
                    x = middleTileX + (GameConstants.TILE_WIDTH);
                    break;
                case 5:
                    x = middleTileX + (2 * GameConstants.TILE_WIDTH);
                    break;
            }
            return x;
        }

        /// <summary>
        /// Generates a tile on a random position, if spawnOnPlayer is true, the tile will be in the player's current lane.
        /// </summary>
        /// <param name="spawnOnPlayer"></param>
        /// <returns></returns>
        private Vector2 generateTilePosition(bool spawnOnPlayer)
        {
            int randomNumber = random.Next(5);
            int x;
            int y;
            switch (spawnOnPlayer ? currentLane : randomNumber)
            {
                case 0:
                    x = getTileXCoord(1);
                    break;
                case 1:
                    x = getTileXCoord(2);
                    break;
                case 2:
                    x = getTileXCoord(3);
                    break;
                case 3:
                default:
                    x = getTileXCoord(4);
                    break;
                case 4:
                    x = getTileXCoord(5);
                    break;
            }
            y = -random.Next(GameConstants.TILE_HEIGHT, GameConstants.WINDOW_HEIGHT);

            return new Vector2(x, y);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            // Draw background.
            spriteBatch.Draw(backgroundImage, new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height), Color.White);

            // Draw score.
            spriteBatch.DrawString(font, "SCORE: ", new Vector2(1600, 300), GameConstants.colorText);
            spriteBatch.DrawString(font, score.ToString(), new Vector2(1600, 350), GameConstants.colorTextNumber);

            // Draw current level.
            spriteBatch.DrawString(font, "LEVEL: ", new Vector2(1600, 500), GameConstants.colorText);
            spriteBatch.DrawString(font, level.ToString(), new Vector2(1600, 550), GameConstants.colorTextNumber);

            // Draw boost and boost bar.
            spriteBatch.DrawString(font, "BOOST: ", new Vector2(10, 350), GameConstants.colorText);
            spriteBatch.DrawString(hugefont, boostAmount.ToString(), new Vector2(10, 400), GameConstants.colorTextNumber);
            spriteBatch.Draw(boostTexture, boostRectangle, Color.White);

            // If boost amount is 100 or above, show "MAX BOOST".
            if (boostAmount >= 100)
            {
                spriteBatch.DrawString(font, "MAX BOOST", new Vector2(10, 570), GameConstants.colorText);
            }

            // Draw tiles.
            foreach (Tile tile in tiles)
            {
                spriteBatch.Draw(tile.image, tile.position, GameConstants.colorTile);
            }

            spriteBatch.Draw(player.image, player.position, GameConstants.colorPlayer);

            if (colorEventEnabled)
            {
                if (Program.ev3Messenger.IsConnected)
                    spriteBatch.DrawString(bigfont, "BOOST ON " + getColorNameByNumber(colorForBoost).ToUpper() + " " + colorBoostCountDown.ToString() + "s", new Vector2(500, 200), colorEventColor());
                else
                    spriteBatch.DrawString(bigfont, "BOOST IN LANE (" + colorForBoost + ") " + colorBoostCountDown.ToString() + "s", new Vector2(500, 200), colorEventColor());
            }

            if (gamePaused)
            {
                spriteBatch.DrawString(bigfont, "GAME PAUSED", new Vector2(700, 400), Color.Red);
            }

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
