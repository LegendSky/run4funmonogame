using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Run4Fun.Sprites
{
    class GameConstants
    {
        public const int playerWidth = 100,
            playerHeight = 100;

        public const int TILE_WIDTH = 230,
            TILE_HEIGHT = 500,
            WINDOW_WIDTH = 1920,
            WINDOW_HEIGHT = 1080;

        public const bool collisionEnabled = true;
        public const int playerSpeedAcceleration = 10; // 10 or 23
        public const bool drawPlayerEnabled = true;
        public const bool drawFallingParticles = true;
        public const int particleSpeed = 30;

        public static readonly Color colorText = Color.Gold;
        public static readonly Color colorTextNumber = Color.Gold;
        public static readonly Color colorPause = Color.Red;
        public static readonly Color colorLeveled = Color.Green;
        public static readonly Color colorTile = Color.White;
        public static readonly Color colorPlayer = Color.Red;
        public static readonly Color colorParticles = Color.Gray;
    }
}
