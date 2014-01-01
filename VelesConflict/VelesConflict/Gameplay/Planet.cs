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


namespace VelesConflict.Gameplay
{
    public class Planet
    {

        public Vector2 Position
        {
            get;
            set;
        }
        public Texture2D Texture
        {
            get;
            set;
        }
        public float PlanetSize
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }
        public PlayerType Owner
        {
            get;
            set;
        }
        public int Forces
        {
            get;
            set;
        }
        public bool HasPeople
        {
            get;
            set;
        }
        public Action Captured
        {
            get;
            set;
        }
        public int Growth
        {
            get;
            set;
        }
        public int GrowthCounter
        {
            get;
            set;
        }
        public int GrowthReset
        {
            get;
            set;
        }
        public float SelectionRotation
        {
            get;
            set;
        }

        public Planet()
        {
            Position = Vector2.Zero;
            Id = 0;
            Owner = PlayerType.Neutral;
            Forces = 0;
            Growth = 0;
            SelectionRotation = 0f;
        }
    }
}
