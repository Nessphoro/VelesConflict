using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VelesConflict.Parsing
{
    class TextManagerOperation
    {
        public string TextFragment { get; set; }

        public SpriteFont Font { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
    }
}
