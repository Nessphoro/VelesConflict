using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using VelesConflict.Shared;
using Microsoft.Xna.Framework;
using VelesConflict.Animators;

namespace VelesConflict.UI
{
    class Menu
    {
        public float BGDepth { get; set; }
        public Texture2D Overlay
        {
            get;
            set;
        }
        public List<IButton> Buttons
        {
            get;
            private set;
        }
        public IAnimator<Vector2> OutVectorAnimator { get; set; }
        public IAnimator<Vector2> InVectorAnimator { get; set; }
        public Menu(IAnimator<Vector2> InAnimator, IAnimator<Vector2> OutAnimator)
        {
            Buttons = new List<IButton>();
            OutVectorAnimator = OutAnimator;
            InVectorAnimator = InAnimator;
        }


        public void Draw()
        {
            foreach (IButton button in Buttons)
            {
                button.Draw();
            }
            Globals.SpriteBatch.Draw(Overlay, Vector2.Zero,null,Color.White,0f,Vector2.Zero,1f,SpriteEffects.None,BGDepth);
        }
    }
}
