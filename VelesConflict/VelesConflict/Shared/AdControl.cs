using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atom.Shared.GUI.Controls;
using SOMAWP7;
using Microsoft.Xna.Framework.Graphics;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Advertising.Mobile.Xna;
using Atom.Shared.Services;
using System.Threading;

namespace VelesConflict.Shared
{
    class AdManager
    {
        public Texture2D Texture { get; private set; }
        public SomaAd Ad { get; private set; }
        public DrawableAd XnaAd { get; private set; }
        string CurrentImageFile = "";

        public AdManager(SomaAd ad)
        {
            Ad = ad;
            ad.NewAdAvailable += new SomaAd.OnNewAdAvailable(ad_NewAdAvailable);
            ad.GetAd();
        }
        public AdManager(DrawableAd ad)
        {
            XnaAd = ad;
            ad.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(ad_ErrorOccurred);
        }

        void ad_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            //Microsoft.Xna.Framework.GamerServices.Guide.BeginShowMessageBox("Error", e.Error.Message, new string[] { "Ok" }, 0, Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.Error, null, null);
        }
        public void Initialize()
        {
            Atom.Shared.Globals.Engine.OnUpdateStart += new Action<GameTime>(Engine_OnUpdateStart);
            Atom.Shared.Globals.Engine.OnDrawEnd += new Action<GameTime>(Engine_OnDrawEnd);
        }

        void Engine_OnUpdateStart(GameTime obj)
        {
            if (IsDrawn && AdGameComponent.Initialized)
                AdGameComponent.Current.Update(obj);
            //XnaAd.Visible = IsDrawn;
            IsDrawn = false;
        }

        void Engine_OnDrawEnd(GameTime obj)
        {
            if (IsDrawn && AdGameComponent.Initialized)
            {

                ISpriteBatchService isb = Atom.Shared.Globals.Engine.GetService(typeof(ISpriteBatchService)) as ISpriteBatchService;
                isb.SpriteBatch.Begin();
                AdGameComponent.Current.Draw(obj);
                isb.SpriteBatch.End();
            }
        }
        bool IsDrawn = false;



        void ad_NewAdAvailable(object sender, EventArgs e)
        {
            if (Ad.Status == "success" && Ad.AdImageFileName != null && Ad.ImageOK)
            {
                try
                {
                    if (CurrentImageFile != Ad.AdImageFileName)
                    {
                        CurrentImageFile = Ad.AdImageFileName;
                        IsolatedStorageFile myIsoStore = IsolatedStorageFile.GetUserStoreForApplication();
                        IsolatedStorageFileStream myAd = new IsolatedStorageFileStream(Ad.AdImageFileName, FileMode.Open, myIsoStore);
                        Texture2D intr = Texture2D.FromStream((Atom.Shared.Globals.Engine as GameBase).GraphicsDevice, myAd);
                        
                        lock (this)
                        {
                            if(Texture!=null)
                                Texture.Dispose();
                            Texture = intr;
                        }

                        myAd.Dispose();
                        myIsoStore.Dispose();
                    }
                }
                catch(Exception ex)
                {
                    string s = ex.Message;
                }
            }
        }
        public void OnTap()
        {
            Microsoft.Phone.Tasks.WebBrowserTask webBrowserTask = new Microsoft.Phone.Tasks.WebBrowserTask();
            webBrowserTask.Uri = new Uri(Ad.Uri);
            webBrowserTask.Show();
        }
        public void Draw(SpriteBatch sb, Vector2 position, Vector2 origin, float depth)
        {
            lock (this)
            {
                IsDrawn = true;
                if (Ad != null)
                {
                    if (Texture != null)
                    {
                        sb.Draw(Texture, position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, depth);
                    }
                }
                else
                {
                    XnaAd.DisplayRectangle = new Rectangle((int)position.X, (int)position.Y, XnaAd.DisplayRectangle.Width, XnaAd.DisplayRectangle.Height);
                }
            }
        }
    }

    class AdControl:Control
    {
        AdManager manager;
        
        public AdControl(AdManager manager)
        {
            this.manager = manager;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            manager.Draw(spriteBatch,Position,Origin,Depth);
        }

        public override bool Intersects(Microsoft.Xna.Framework.Vector2 position)
        {
            lock (manager)
            {
                if (manager.Texture!=null)
                {
                    Rectangle rectangle = new Rectangle((int)Position.X, (int)Position.Y, manager.Texture.Width, manager.Texture.Height);
                    Rectangle iret = new Rectangle((int)position.X, (int)position.Y, 5, 5);
                    if (rectangle.Intersects(iret))
                    {
                        manager.OnTap();   
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
