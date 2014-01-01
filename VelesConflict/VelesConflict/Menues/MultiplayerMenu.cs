using Atom.Phone.TCP;
using Atom.Shared;
using Atom.Shared.Animators;
using Atom.Shared.GUI;
using Atom.Shared.GUI.Controls;
using Atom.Shared.Services;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PGGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelesConflict.Shared;

namespace VelesConflict.Menues
{
    class MultiplayerMenu:Menu
    {
        Image progressBar;
        IResourceCacheService RCS;
        WebClient WebClient;
        Client MasterClient;
        PGGIProvider Provider;
        bool Initialized = false;
        

        public MultiplayerMenu()
            : base()
        {


            RCS = Globals.Engine.GetService<IResourceCacheService>();
            IAnimator<Vector2> MainAnimatorOut = new SmoothVectorAnimator();
            MainAnimatorOut.AtStart = Vector2.Zero;
            MainAnimatorOut.AtEnd = new Vector2(-120, 425);
            MainAnimatorOut.Duration = 450;

            IAnimator<Vector2> MainAnimatorIn = new LinearVectorAnimator();
            MainAnimatorIn.AtStart = new Vector2(120, -425);
            MainAnimatorIn.AtEnd = Vector2.Zero;
            MainAnimatorIn.Duration = 375;

            InVectorAnimator = MainAnimatorIn;
            OutVectorAnimator = MainAnimatorOut;
            IResourceObject<Texture2D> MainOverlay = RCS.GetObject<Texture2D>("Menues/Main/MainOverlay");
            IResourceObject<Texture2D> MainButton = RCS.GetObject<Texture2D>("Menues/Main/button");
            RCS.PreCache<Texture2D>("Menues/Multiplayer/ProgressWheel");

            this.MessageBox = GameBase.BuildMessageBox(new Vector2(300,100));
            this.MessageBox.Parent = this;
            this.MessageBox.OnClosed += MessageBox_OnClosed;

            Controls.Add(new Image() { Texture = MainOverlay, Position = Vector2.Zero, Depth = 0.5f });
            Controls.Add(new AdControl(GameBase.adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
                tb.Text = LocalizationData.QuickMatch;
                tb.TextPosition = new Vector2(15, -47);
                tb.OnClick += tb_OnClick;
                //tb.Sound = MenuItemSelected.CreateInstance();
                //tb.UseSound = true;
                tb.Font = GameBase.Font;
                tb.Depth = 0.9f;
                tb.HoverColor = new Color(50, 238, 50);
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                //.NextMenu = LocalizationData.QuickMatch;
                Controls.Add(tb);
            }
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
                tb.Text = LocalizationData.Back;
                tb.TextPosition = new Vector2(15, -47);
                //tb.Sound = MenuItemSelected.CreateInstance();
                //tb.UseSound = true;
                tb.Font = GameBase.Font;
                tb.Depth = 0.9f;
                tb.HoverColor = new Color(50, 238, 50);
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                tb.OnClick += OnBack;
                tb.NextMenu = "%Back%";
                Controls.Add(tb);
            }
            progressBar = new Image();
            progressBar.Texture = RCS.GetObject<Texture2D>("Menues/Multiplayer/ProgressWheel");
            progressBar.Origin = new Vector2(25, 25);
            progressBar.Position = new Vector2(400, 230);
            progressBar.Scale = 1f;
            progressBar.Visible = false;
            progressBar.Enabled = true;
            
            InitializeControls();
        }

        void MessageBox_OnClosed()
        {
            Packet m = new Packet();
            m.Write((byte)1);
            MasterClient.SendPacket(m);
        }
        
        void OnBack(object sender, EventArgs e)
        {
            MasterClient.Disconnect();
            
        }
        public override void OnEntering(object sender, MenuChangeEventArgs e)
        {
            Initialized = false;
            IUpdateService ius = Globals.Engine.GetService<IUpdateService>();
            ius.Subscribe(Update);
            base.OnEntering(sender, e);
        }
        public override void OnExiting(object sender, MenuChangeEventArgs e)
        {
            Initialized = false;
            IUpdateService ius = Globals.Engine.GetService<IUpdateService>();
            ius.Unsubscribe(Update);
            base.OnExiting(sender, e);
        }
        void Update(GameTime gt)
        {
            if(progressBar!=null && progressBar.Visible)
            {                
                progressBar.Rotation += MathHelper.PiOver4*(float)gt.ElapsedGameTime.TotalSeconds;
                progressBar.Scale = this.MessageBox.Scale;
            }
        }
        void Initialize()
        {
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {

                progressBar.Visible = true;
                this.MessageBox.Text = "\n\n\n\n" + LocalizationData.Connecting;
                this.MessageBox.Locked = true;
                this.MessageBox.Show(true);

                Provider = new PGGIProvider();
                Provider.Application = "Veles Conflict";
                Provider.UserAccessKey = "27cd4d8c0bb261f73a22c6e27f4c1592e5aa561be578815dfd4188fddeabf9ab";
                Provider.PGGI = GameBase.DeviceID;
                Provider.Password = GameBase.DeviceID;

                if (GameBase.playerData.IsStored)
                {
                    Provider.BeginLogin(false, LoginComplete, null);
                }
                else
                {
                    GameBase.playerData.IsStored = true;
                    GameBase.playerData.OGI = GameBase.playerData.Hash = GameBase.DeviceID;
                    Provider.BeginRegister(Provider.PGGI, Provider.Password, null, RegistrationComplete, null);
                }
            }
            else
            {
                progressBar.Visible = true;
                this.MessageBox.Text = LocalizationData.ConnectionRequired;
                this.MessageBox.Locked = false;
                this.MessageBox.Show(true);
            }
        }
        void tb_OnClick(object sender, EventArgs e)
        {
            if(!Initialized)
            {
                Initialized = true;
                Initialize();
            }
            else
            {
                if(this.MessageBox.Visible==false)
                {
                    this.MessageBox.Show(true);
                }
                this.MessageBox.Text = "\n\n\n\n" + LocalizationData.LookingForGames;
                Packet m = new Packet();
                m.Write((byte)0);
                MasterClient.SendPacket(m);
            }
        }

        void SetRightsComplete(IAsyncResult result)
        {
            Provider.EndSetRights(result);
            CreateMaster();
        }
        void LoginComplete(IAsyncResult result)
        {
            Provider.EndLogin(result);
            Provider.BeginSetRights(true, SetRightsComplete, null);
            
        }
        void RegistrationComplete(IAsyncResult result)
        {
            Provider.EndRegister(result);
            Provider.BeginLogin(false, LoginComplete, null);
        }

        void CreateMaster()
        {
            MasterClient = new Client();
            MasterClient.OnConnection = OnMasterConnection;
            MasterClient.OnPacketRecieved = OnMasterPacket;

            Packet ConnectionPacket = new Packet();
            ConnectionPacket.Write(Provider.PGGI);
            ConnectionPacket.Write(Provider.Token);
            MasterClient.Connect(new DnsEndPoint("master.velesconflict.com", 33605), ConnectionPacket);
        }

        void OnMasterConnection(object sender, ConnectionArgs e)
        {
            
            int Score = 0;
            IAsyncResult result = Provider.BeginGetData("Score", null, null);
            result.AsyncWaitHandle.WaitOne();
            Score = Provider.EndGetData<int>(result).Data;

            this.MessageBox.Text = "\n\n\n\n" + LocalizationData.LookingForGames;
            Packet m = new Packet();
            m.Write((byte)0);
            MasterClient.SendPacket(m);

            MessageBox.Locked = false;
        }
        void OnMasterPacket(object sender, PacketRecievedArgs e)
        {
            byte Type = e.Packet.ReadByte();
            switch (Type)
            {
                case 0xFF:
                    //
                    break;
                case 0xFE:
                    progressBar.Visible = true;
                    MessageBox.Locked = true;
                    MessageBox.Show(true);
                    this.MessageBox.Text = "\n\n\n\n "+LocalizationData.GameFound;
                    string Server = e.Packet.ReadString();
                    int Port = e.Packet.ReadInt();
                    StartNewGame(Server, Port);
                    break;
            }
        }

        private void StartNewGame(string Server, int Port)
        {
            GameBase.Multiplayer = new MultiplayerBase(Server, Port,Provider);
            (Globals.Engine as GameBase).DisableMenues();
            GameBase.GameState = GameState.Multiplayer;
            progressBar.Visible = false;
            MessageBox.Locked = false;
            MessageBox.Close();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch SpriteBatch)
        {
            base.Draw(SpriteBatch);
            if (progressBar.Visible)
            {
                progressBar.Draw(SpriteBatch);
            }
        }
    }
}
