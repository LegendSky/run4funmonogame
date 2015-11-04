using EV3MessengerLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Run4Fun.Sprites;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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

        private Rectangle energyRectangle;
        private Texture2D energyBarTexture;
        private Texture2D fallingParticleTexture;

        private int tileSpeed = 10;

        // The current tile the player is on.
        private int currentLane;

        private int score = 0;
        private int level = 1;
        private Player player;
        private List<Tile> tiles = new List<Tile>();
        private List<FallingParticle> particles = new List<FallingParticle>();

        private Random random = new Random();

        private bool dashEnabled = false;
        private int energyAmount = GameConstants.START_ENERGY_AMOUNT;
        private int colorForEnergy;
        private bool colorEventEnabled = false;
        private int colorEnergyCountDown = 5;

        private int oneSecondTimer = 0, twoSecondTimer = 0, tileGenerationTimer = 0, twentySecondTimer = 0, thirtySecondTimer = 0, tenthSecondTimer = 0, beginGameTimer = 0;
        private bool recentlyLeveled = false;

        // Milliseconds before spawning next wave.
        private int tileGenerationFrequency = 2500;

        private int playerSpeed = 0;
        private int newPositionX;

        private bool gamePaused = false;

        private Song gameSong;

        private bool gameJustStarted = true;

        public Run4FunGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            //graphics.ToggleFullScreen();
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
            updateEnergyBar();

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
            player = new Player(Content.Load<Texture2D>("player"), new Vector2((GameConstants.WINDOW_WIDTH / 2) - (GameConstants.PLAYER_WIDTH / 2), GameConstants.WINDOW_HEIGHT - GameConstants.PLAYER_START_HEIGHT_OFFSET));
            energyBarTexture = Content.Load<Texture2D>("energy");

            fallingParticleTexture = Content.Load<Texture2D>("fallingparticle");

            // Load fonts.
            smallfont = Content.Load<SpriteFont>("smallfont");
            font = Content.Load<SpriteFont>("font");
            bigfont = Content.Load<SpriteFont>("bigfont");
            hugefont = Content.Load<SpriteFont>("hugefont");

            gameSong = Content.Load<Song>("gamesong");
            startPlayingGameSong();
            // Sound
            // soundEffect = Content.Load<SoundEffect>("gamesound.mp3");
            //  soundEffect.Play();
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

            // Descent particles.
            descentParticles();

            // Increase score.
            increaseScore();

            // Check if current tile is 
            if (!Program.ev3Messenger.IsConnected && colorEventEnabled && currentLaneIsEnergyColor())
            {
                addEnergyAndScoreAndUpdate();
            }

            if (recentlyLeveled)
            {
                twoSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (twoSecondTimer >= 2000)
                {
                    twoSecondTimer = 0;
                    recentlyLeveled = false;
                }
            }

            // Every tenth second.
            tenthSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (tenthSecondTimer >= 50)
            {
                spawnAndRemoveParticles();

                if (dashEnabled)
                {
                    if (energyAmount <= 0)
                    {
                        dashEnabled = false;
                    }

                    tenthSecondTimer = 0;
                    energyAmount--;
                    updateEnergyBar();
                }
            }

            // To clear left-over messages from last game.
            if (gameJustStarted)
            {
                beginGameTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (beginGameTimer >= 100)
                {
                    gameJustStarted = false;
                }
            }

            // Every second.
            oneSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (oneSecondTimer >= 1000)
            {
                oneSecondTimer = 0;
                if (colorEventEnabled)
                {
                    if (colorEnergyCountDown <= 0)
                    {
                        resetColorEvent();
                    }
                    else
                    {
                        colorEnergyCountDown--;
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
            thirtySecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (thirtySecondTimer >= 30000)
            {
                thirtySecondTimer = 0;

                levelUp();

                // Increase spawn frequency.
                if (tileGenerationFrequency > 1000)
                {
                    tileGenerationFrequency -= 100;
                }
            }

            // Every 20 seconds.
            twentySecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (twentySecondTimer >= 20000)
            {
                twentySecondTimer = 0;
                startColorEnergyEvent();
            }

            if (player.position.X == newPositionX)
            {
                playerSpeed = 0;
            }
            player.position.X += playerSpeed;

            base.Update(gameTime);
        }

        /// <summary>
        /// Update the energy bar.
        /// </summary>
        private void updateEnergyBar()
        {
            energyRectangle = new Rectangle(10, 520, energyAmount * 3, 50);
        }

        /// <summary>
        /// Increase level and tilespeed.
        /// </summary>
        private void levelUp()
        {
            level++;
            tileSpeed += 2;
            recentlyLeveled = true;
        }

        /// <summary>
        /// Reset color event.
        /// </summary>
        private void resetColorEvent()
        {
            colorEventEnabled = false;
            colorEnergyCountDown = 5;
            colorForEnergy = 0;
        }

        /// <summary>
        /// Checkes whether the color is in the current lane.
        /// </summary>
        /// <returns></returns>
        private bool currentLaneIsEnergyColor()
        {
            return colorForEnergy == currentLane;
        }

        /// <summary>
        /// Start color energy event.
        /// </summary>
        private void startColorEnergyEvent()
        {
            colorForEnergy = random.Next(1, 6);

            // Energy shouldn't be on player's position.
            while (currentLaneIsEnergyColor())
            {
                colorForEnergy = random.Next(5) + 1;
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
                        if (!gameJustStarted)
                        {
                            moveLeftPc();
                        }
                    }
                    else if (message.ValueAsText == "Right")
                    {
                        if (!gameJustStarted)
                        {
                            moveRightPc();
                        }
                    }
                }
                else if (message != null && message.MailboxTitle == "Color")
                {
                    int color = (int)message.ValueAsNumber;
                    Console.WriteLine(color);
                    if (colorEventEnabled && colorForEnergy == color)
                    {
                        addEnergyAndScoreAndUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Increase energy and score amount, reset the color event and update energy bar.
        /// </summary>
        private void addEnergyAndScoreAndUpdate()
        {
            if (energyAmount <= 100)
            {
                energyAmount += 10;
            }

            score += 5000;
            resetColorEvent();
            updateEnergyBar();
        }

        /// <summary>
        /// Spawn new tiles and remove tiles out of vision.
        /// </summary>
        private void spawnAndRemoveTiles()
        {
            // 1 to 3 tiles will spawn on random lanes.
            int repeat = random.Next(3) + 1;

            // Spawn completely random.
            /*for (int i = 0; i < repeat; i++)
            {
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition(false)));
            }*/

            // Spawn without duplicates.
            foreach (int x in generateXCoordinatesWithoutDupes(repeat))
            {
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), new Vector2(x, getRandomYCoordinate())));
            }

            // 50% chance to spawn tile on the player's lane.
            if ((random.Next(2) + 1) == 1)
            {
                tiles.Add(new Tile(Content.Load<Texture2D>("bigtile"), generateTilePosition(true)));
            }

            removeOverlappingTiles();

            // Remove out of vision tiles.
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].position.Y > 1080)
                    tiles.Remove(tiles[i]);
            }
        }

        private List<int> generateXCoordinatesWithoutDupes(int amount)
        {
            int size = 5;
            if (amount > size)
            {
                throw new IndexOutOfRangeException("Amount too high, should only be up to 5.");
            }

            List<int> listNumbers = new List<int>(size);
            for (int i = 1; i <= size; i++)
            {
                listNumbers.Add(i);
            }

            List<int> listXCoordinates = new List<int>(amount);
            for (int i = 0; i < amount; i++)
            {
                int index = random.Next(listNumbers.Count);
                int randomNum = listNumbers[index];
                int x = getTileXCoord(randomNum);
                listNumbers.RemoveAt(index);
                listXCoordinates.Add(x);
            }
            return listXCoordinates;
        }

        /// <summary>
        /// Spawn new particles and remove particles out of vision.
        /// </summary>
        private void spawnAndRemoveParticles()
        {
            particles.Add(new FallingParticle(Content.Load<Texture2D>("fallingparticle"), generateParticlePosition()));

            // Remove out of vision particles.
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].position.Y > 1080)
                    particles.Remove(particles[i]);
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
                if (gamePaused)
                {
                    gamePaused = false;
                    startPlayingGameSong();
                }
                else
                {
                    gamePaused = true;
                    stopPlayingSound();
                }

                // gamePaused = !gamePaused;

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

            if (dashPressed() && energyAmount > 0)
            {
                dashEnabled = true;
            }
            else
            {
                dashEnabled = false;
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
            if (!GameConstants.COLLISION_ENABLED || dashEnabled)
            {
                return;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                if (playerAndTileCollide(player, tiles[i]))
                {
                    putEV3InOriginalPosition();
                    gameJustStarted = true;
                    stopPlayingSound();
                    new UsernameForm(score).ShowDialog();
                    Environment.Exit(0);
                }
            }
        }
    
        private void putEV3InOriginalPosition()
        {
          //  readEV3MessageAndDoAction();
            while (currentLane != (int)lanePlayer.LANE_3_GREEN)
            {
                if (currentLane > (int)lanePlayer.LANE_3_GREEN)
                {
                    currentLane -= 1;
                    moveLeftEV3();
                }
                else
                {
                    currentLane += 1;
                    moveRightEV3();
                }
            }
        }

        private void startPlayingGameSong()
        {
            MediaPlayer.Play(gameSong);
        }

        /// <summary>
        /// Stop the mediaplayer.
        /// </summary>
        private void stopPlayingSound()
        {
            MediaPlayer.Stop();
        }

        /// <summary>
        /// Descent all tiles, speed depends on tileSpeed.
        /// </summary>
        private void descentTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].position.Y += dashEnabled ? tileSpeed * 5 : tileSpeed;
            }
        }

        /// <summary>
        /// Descent all particles, speed depends on particleSpeed.
        /// </summary>
        private void descentParticles()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].position.Y += GameConstants.particleSpeed;
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
        /// Checks whether A or Space is held.
        /// </summary>
        /// <returns></returns>
        private bool dashPressed()
        {
            bool aPressed = gamePadState.Buttons.A == ButtonState.Pressed;
            bool aReleased = gamePadState.Buttons.A == ButtonState.Released;

            bool spacePressed = keyState.IsKeyDown(Keys.Space);
            bool spaceReleased = keyState.IsKeyUp(Keys.Space);

            return (aPressed && !aReleased) || (spacePressed && !spaceReleased);
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
        /// Checks if tiles collide with eachother, and if so remove the colliding tile.
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        private void removeOverlappingTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile1 = tiles[i];
                Rectangle tile1Rectangle = new Rectangle((int)tile1.position.X, (int)tile1.position.Y, tile1.image.Width, tile1.image.Height);

                for (int j = 0; j < tiles.Count; j++)
                {
                    Tile tile2 = tiles[j];
                    Rectangle tile2Rectangle = new Rectangle((int)tile2.position.X, (int)tile2.position.Y, tile2.image.Width, tile2.image.Height);
                    if (tile1Rectangle.Intersects(tile2Rectangle) && tile1Rectangle != tile2Rectangle)
                    {
                        tiles.RemoveAt(j);
                    }
                }
            }
        }

        /// <summary>
        /// Increase score.
        /// </summary>
        private void increaseScore()
        {
            score++;
        }

        /// <summary>
        /// The color for energy event.
        /// </summary>
        /// <returns></returns>
        private Color colorEventColor()
        {
            Color color;
            switch (colorForEnergy)
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
                    //color = Color.Yellow;
                    color = Color.DarkOrange;
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
            int randomNumber = random.Next(5) + 1;
            int x;
            int y;
            switch (spawnOnPlayer ? currentLane : randomNumber)
            {
                case 1:
                    x = getTileXCoord(1);
                    break;
                case 2:
                    x = getTileXCoord(2);
                    break;
                case 3:
                    x = getTileXCoord(3);
                    break;
                case 4:
                default:
                    x = getTileXCoord(4);
                    break;
                case 5:
                    x = getTileXCoord(5);
                    break;
            }
            y = -random.Next(GameConstants.TILE_HEIGHT, 2 * GameConstants.WINDOW_HEIGHT);

            return new Vector2(x, y);
        }

        private int getRandomYCoordinate()
        {
            return -random.Next(GameConstants.TILE_HEIGHT, GameConstants.WINDOW_HEIGHT);
        }

        /// <summary>
        /// Generates particle on a random position.
        /// </summary>
        /// <returns></returns>
        private Vector2 generateParticlePosition()
        {
            int randomNumber = random.Next(5);
            int x;
            int y;
            x = random.Next(0, GameConstants.WINDOW_WIDTH);
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
            spriteBatch.DrawString(font, "SCORE: ", new Vector2(1600, 300), GameConstants.TEXT_COLOR);
            spriteBatch.DrawString(font, score.ToString(), new Vector2(1600, 350), GameConstants.NUMBER_COLOR);

            // Draw current level.
            spriteBatch.DrawString(font, "LEVEL: ", new Vector2(1600, 500), GameConstants.TEXT_COLOR);
            spriteBatch.DrawString(font, level.ToString(), new Vector2(1600, 550), GameConstants.NUMBER_COLOR);

            // Draw energy and energy bar.
            spriteBatch.DrawString(font, "ENERGY: ", new Vector2(10, 350), GameConstants.TEXT_COLOR);
            spriteBatch.DrawString(hugefont, energyAmount.ToString(), new Vector2(10, 400), GameConstants.NUMBER_COLOR);
            spriteBatch.Draw(energyBarTexture, energyRectangle, Color.White);

            // If energy amount is 100 or above, show "MAX ENERGY".
            if (energyAmount >= 100)
            {
                spriteBatch.DrawString(font, "MAX ENERGY", new Vector2(10, 570), GameConstants.TEXT_COLOR);
            }

            // Draw tiles.
            foreach (Tile tile in tiles)
            {
                spriteBatch.Draw(tile.image, tile.position, GameConstants.TILE_COLOR);
            }

            // Draw player.
            if (GameConstants.DRAW_PLAYER_ENABLED)
            {
                spriteBatch.Draw(player.image, player.position, GameConstants.PLAYER_COLOR);
            }

            // Draw particles.
            if (GameConstants.drawFallingParticles && dashEnabled)
            {
                foreach (FallingParticle particle in particles)
                {
                    spriteBatch.Draw(fallingParticleTexture, particle.position, GameConstants.PARTICLE_COLOR);
                }
            }

            if (colorEventEnabled)
            {
                if (Program.ev3Messenger.IsConnected)
                {
                    drawTextInMiddle("ENERGY ON " + getColorNameByNumber(colorForEnergy).ToUpper() + " " + colorEnergyCountDown.ToString() + "s", bigfont, 200, colorEventColor());
                }
                else
                {
                    drawTextInMiddle("ENERGY IN LANE (" + colorForEnergy + ") " + colorEnergyCountDown.ToString() + "s", bigfont, 200, colorEventColor());
                }
            }

            if (gamePaused)
            {
                drawTextInMiddle("GAME PAUSED", bigfont, 400, GameConstants.PAUSE_COLOR);
            }

            if (recentlyLeveled)
            {
                drawTextInMiddle("LEVEL " + level, bigfont, 300, GameConstants.LEVEL_UP_COLOR);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawTextInMiddle(string text, SpriteFont font, int y, Color color)
        {
            int middleX = (GameConstants.WINDOW_WIDTH / 2) - (int)(font.MeasureString(text).Length() / 2);
            spriteBatch.DrawString(font, text, new Vector2(middleX, y), color);
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            putEV3InOriginalPosition();
            stopPlayingSound();
            new StartForm().ShowDialog();
        }
    }
}
