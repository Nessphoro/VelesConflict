using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atom.Shared.GUI.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Atom.Shared.Services;

namespace VelesConflict.Shared
{
    class HexControl:Control
    {
        Cell[] Cells;
        IResourceObject<Texture2D> Texture;
        
        public HexControl(IResourceObject<Texture2D> Texture, Cell[] cells)
        {
            
            Cells = cells;
            foreach (Cell c in cells)
            {
                c.Color = Color.White * 0.8f;
                
            }
            Scale = 1;
            this.Texture = Texture;
            Origin = new Vector2(Texture.Instance.Width / 2, Texture.Instance.Height / 2);
        }
        public Cell this[int index]
        {
            get
            {
                return Cells[index];
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Texture2D t=Texture.Instance;
            foreach (Cell cell in Cells)
            {
                cell.DrawPosition=Position+Offset + cell.Position;
            }
            foreach (Cell cell in Cells)
            {
                spriteBatch.Draw(t,cell.DrawPosition , null, cell.Color, 0f, Origin, Scale, SpriteEffects.None, Depth);
            }
        }

        public override bool Intersects(Microsoft.Xna.Framework.Vector2 position)
        {
            
            foreach (Cell cell in Cells)
            {
                if (Vector2.Distance(cell.Position+Position+Offset, position)<64)
                {
                    cell.Click();
                    return true;
                }
            }

            return false;
        }
    }
}
