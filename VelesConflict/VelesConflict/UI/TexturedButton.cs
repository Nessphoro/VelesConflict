using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelesConflict.Shared;

namespace VelesConflict.UI
{
    class TexturedButton:IButton
    {
        protected Vector2 _position;
        protected Texture2D _normal;
        protected Texture2D _hovering;
        protected GameButtonState _buttonState;

        public TexturedButton(Texture2D Normal, Texture2D Hovering, Vector2 Position)
        {
            _position = Position;
            this.Scale = 1f;
            _normal = Normal;
            _hovering = Hovering;
           // _origin = new Vector2(_normal.Width / 2, _normal.Height / 2);
        }

        public Vector2 Offset
        {
            get;
            set;
        }
        /// <summary>
        /// Get button position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _position + Offset;
            }
            set
            {
                _position = value - Offset;
            }
        }
        /// <summary>
        /// Position offset
        /// </summary>
        public Vector2 Origin
        {
            get;
            set;
        }
        /// <summary>
        /// Get a texture that is needed to be drawn in current state
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                switch (_buttonState)
                {
                    case GameButtonState.Normal:
                        return _normal;

                    case GameButtonState.Hovering:
                        return _hovering;
#if !WINDOWS_PHONE
                    case GameButtonState.Click:
                        return _click;
#endif
                    default:
                        return _normal;
                }
            }
        }
        /// <summary>
        /// Intersection rectangle, offset by position and origin
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                Rectangle ret = _normal.Bounds;
                ret.Width = (int)(ret.Width / Scale);
                ret.Height = (int)(ret.Height / Scale);
                ret.X = (int)Position.X - (int)(Origin.X / Scale);
                ret.Y = (int)Position.Y - (int)(Origin.Y / Scale);
                return ret;
            }
        }
        /// <summary>
        /// State 
        /// </summary>
        public GameButtonState ButtonState
        {
            get { return _buttonState; }
            set { _buttonState = value; }
        }
        /// <summary>
        /// Action to be called on click
        /// </summary>
        public EventHandler OnClick
        {
            get;
            set;
        }

        public string NextMenu
        {
            get;
            set;
        }

        public float Scale
        {
            get;
            set;
        }
        public object Data
        {
            get;
            set;
        }


        public void Draw()
        {
            Globals.SpriteBatch.Draw(this.Texture, this.Position, null, Color.White, 0f, this.Origin, this.Scale, SpriteEffects.None, 0f);
        }
    }
}
