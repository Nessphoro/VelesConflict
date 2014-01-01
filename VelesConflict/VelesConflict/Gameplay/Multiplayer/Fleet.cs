using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace VelesConflict.Gameplay.Multiplayer
{
    public class Fleet
    {
        public Vector2 Position
        {
            get;
            set;
        }
        public PlayerType Owner
        {
            get;
            set;
        }
        public int Origin
        {
            get;
            set;
        }
        public int Destination
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public float Rotation
        {
            get;
            set;
        }

        public Vector2[] Positions
        {
            get;
            set;
        }

        public Fleet()
        {

        }

        public Fleet(Fleet source)
        {
            Position = source.Position;
            Owner = source.Owner;
            Origin = source.Origin;
            Destination = source.Destination;
            Count = source.Count;
            Rotation = source.Rotation;
            Positions = source.Positions;
        }
    }
}
