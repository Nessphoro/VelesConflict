using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor
{
    public class Camera
    {
        public Camera()
        {
            Zoom = 1.0f;
        }

        public Vector2 Position { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }

        public Matrix GetViewMatrix(Vector2 Parallax,GraphicsDevice graphics)
        {
            return Matrix.CreateTranslation(new Vector3(-Position*Parallax, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(graphics.Viewport.Width >> 1, graphics.Viewport.Height >> 1, 0));
            
        }
    }

}
