using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Run4FunMonogame.Sprites
{
    class Player : Sprite
    {
        public Player(Texture2D image, Vector2 position, int speed) : base(image, position)
        {
            this.image = image;
            this.position = position;
        }
    }
}
