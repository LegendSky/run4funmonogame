using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Run4FunMonogame
{
    class Tile : Sprite
    {
        public Tile(Texture2D image, Vector2 position)
            : base(image, position)
        {
            this.image = image;
            this.position = position;
        }
    }
}