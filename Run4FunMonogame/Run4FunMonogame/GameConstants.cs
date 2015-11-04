using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Run4Fun.Sprites
{
    class GameConstants
    {
        public const int START_ENERGY_AMOUNT = 10;
        public const bool COLLISION_ENABLED = true;
        public const bool DRAW_PLAYER_ENABLED = true;
        public const int PLAYER_START_HEIGHT_OFFSET = 200;

        public const int PLAYER_WIDTH = 100,
            PLAYER_HEIGHT = 100;

        public const int TILE_WIDTH = 200,
            TILE_HEIGHT = 500,
            WINDOW_WIDTH = 1920,
            WINDOW_HEIGHT = 1080;

        public const int playerSpeedAcceleration = 10; // 10 or 23
        public const bool drawFallingParticles = true;
        public const int particleSpeed = 30;

        public static readonly Color TEXT_COLOR = Color.Gold;
        public static readonly Color NUMBER_COLOR = Color.Gold;
        public static readonly Color PAUSE_COLOR = Color.Red;
        public static readonly Color LEVEL_UP_COLOR = Color.Green;
        public static readonly Color TILE_COLOR = Color.White;
        public static readonly Color PLAYER_COLOR = Color.Red;
        public static readonly Color PARTICLE_COLOR = Color.Black;
    }
}
