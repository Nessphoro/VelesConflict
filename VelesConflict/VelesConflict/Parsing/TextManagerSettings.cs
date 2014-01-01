using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VelesConflict.Parsing
{
    class TextManagerSettings
    {
        public int Width { get; set; }
        public SpriteFont Font { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Origin { get; set; }
        public float Depth { get; set; }
        public float Scale { get; set; }

        public TextManagerSettings()
        {
            Offset = Vector2.Zero;
            Origin = Vector2.Zero;
            Scale = 1f;
        }
    }
}
