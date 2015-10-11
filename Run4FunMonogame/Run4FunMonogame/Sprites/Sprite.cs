using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Run4FunMonogame
{
    abstract class Sprite
    {
        protected Texture2D image;
        protected Vector2 position;
        protected int speed;

        public Sprite(Texture2D image, Vector2 position, int speed)
        {
            this.image = image;
            this.position = position;
            this.speed = speed;
        }
    }
}
