using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.UI
{
    enum GameButtonState 
    { 
        Hovering, 
        Normal
    }
    interface IButton
    {
        Vector2 Offset
        {
            get;
            set;
        }
        Vector2 Position
        {
            get;
            set;
        }
        Vector2 Origin
        {
            get;
            set;
        }
        Rectangle Rectangle
        {
            get;
        }
        GameButtonState ButtonState
        {
            get;
            set;
        }
        EventHandler OnClick
        {
            get;
            set;
        }
        string NextMenu
        {
            get;
            set;
        }
        float Scale
        {
            get;
            set;
        }
        object Data
        {
            get;
            set;
        }

        void Draw();
    }
}
