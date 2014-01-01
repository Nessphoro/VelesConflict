using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atom.Shared.GUI;
using Atom.Shared.Animators;
using Microsoft.Xna.Framework;
using Atom.Shared.GUI.Controls;
using VelesConflict.Shared;
using Atom.Shared.Services;
using Atom.Shared;
using Microsoft.Xna.Framework.Graphics;
using VelesConflict.Parsing;
using VelesConflict.Gameplay;

namespace VelesConflict.Menues
{
    class EpilogueMenu:Menu
    {
        TextManager textManager;
        public override string[] GetResources()
        {
            return new string[]{"Menues/Base/Base","Menues/LeftButton","Menues/RightButton","Menues/Base/bg_ext"};
        }

        public EpilogueMenu()
        {
            IResourceCacheService RCS = Globals.Engine.GetService<IResourceCacheService>();
            IAnimator<Vector2> Empty = new SmoothVectorAnimator();
            Empty.AtStart = Vector2.Zero;
            Empty.AtEnd = Vector2.Zero;
            Empty.Duration = 0;

            InVectorAnimator = Empty;
            OutVectorAnimator = Empty;
            Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Base/Base"), Position = Vector2.Zero, Depth = 0.48f });
            Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Base/bg_ext"), Position = new Vector2(0, 0), Depth = 0.6f });
            Controls.Add(new AdControl(GameBase.adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
            HybridButton RightBase = new HybridButton(RCS.GetObject<Texture2D>("Menues/RightButton"), RCS.GetObject<Texture2D>("Menues/RightButton"), new Vector2(757, 444));
            RightBase.Origin = new Vector2(172, 62);
            RightBase.CanOffset = false;
            RightBase.Depth = 0.01f;
            RightBase.NextMenu = "Campaing";
            RightBase.Font = GameBase.Font;
            RightBase.TextPosition = -RightBase.Origin + new Vector2(60, 23);
            RightBase.Text = LocalizationData.Select;
            Controls.Add(RightBase);
            textManager = new TextManager();
            textManager.Settings.Width = 670;
            textManager.Settings.Offset = new Vector2(67, 54);
            textManager.Settings.Font = GameBase.Font;
            textManager.Settings.Depth = 0.49f;
            NotReturnable = true;
            InitializeControls();
        }
        public void MoveText(TouchEventArgs e)
        {
            if(textManager.Height>320)
                textManager.Settings.Offset = Vector2.Clamp(textManager.Settings.Offset + new Vector2(0, e.Direction.Y), new Vector2(67, 54 - (textManager.Height - 320)), new Vector2(67, 54));
            
        }
        public void Prepare(Campaign c)
        {
            textManager.Text = c.Epilogue;
            textManager.Parse();
        }

        public override void Draw(SpriteBatch SpriteBatch)
        {
            base.Draw(SpriteBatch);
            textManager.Draw(SpriteBatch);
        }
    }
}
