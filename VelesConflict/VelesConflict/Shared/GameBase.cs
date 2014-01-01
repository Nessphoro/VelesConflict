using Atom.Graph;
using Atom.Shared;
using Atom.Shared.Animators;
using Atom.Shared.Graphics;
using Atom.Shared.GUI;
using Atom.Shared.GUI.Controls;
using Atom.Shared.Services;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using VelesConflict.AI;
using VelesConflict.Editor;
using VelesConflict.Gameplay;
using VelesConflict.Menues;
using VelesConflict.Parsing;


namespace VelesConflict.Shared
{
    enum PlanetSize
    {
        Large, Medium, Small
    }
    public enum GameState
    {
        Menu, Pause, Singleplayer, Multiplayer,Editor
    }
    public enum ScienceState
    {
        Forward, Reversed
    }
    public enum Difficulty : int
    {
        Easy = 0, Medium = 1, Hard = 2, Insane = 3
    }
    public class PlayerData
    {
        public string OGI { get; set; }
        public string Hash { get; set; }
        public bool IsStored { get; set; }
        public bool DeviceRegistered { get; set; }
        public bool ReportingEnabled { get; set; }
        public bool FirstLoad { get; set; }
        public Difficulty Difficulty { get; set; }
        public Dictionary<string, int> Progress { get; set; }
        public Dictionary<string, int> Research { get; set; }
        public int Points { get; set; }

        public PlayerData()
        {
            IsStored = false;
            Progress = new Dictionary<string, int>();
            Research = new Dictionary<string, int>();
            Difficulty = Difficulty.Easy;
            Research.Add("Attack", 0);
            Research.Add("Defense", 0);
            Research.Add("Speed", 0);
            Research.Add("Growth", 0);
            Points = 2;
        }
        public PlayerData(PlayerData instance)
        {
            IsStored = instance.IsStored;
            if (IsStored)
            {
                OGI = instance.OGI;
                Hash = instance.Hash;
            }
            DeviceRegistered = instance.DeviceRegistered;
            ReportingEnabled = instance.ReportingEnabled;
            FirstLoad = instance.FirstLoad;
            Progress = new Dictionary<string, int>();
            Research = new Dictionary<string, int>();
            Difficulty = Difficulty.Easy;
            Points = 2;
            Research.Add("Attack", 0);
            Research.Add("Defense", 0);
            Research.Add("Speed", 0);
            Research.Add("Growth", 0);
        }
        public int GetProgress(string Name)
        {
            if (Progress.ContainsKey(Name))
                return Progress[Name];
            else
            {
                Progress.Add(Name, 1);
                return 1;
            }
        }
        public void SetProgress(string Name, int Value)
        {
            if (Progress.ContainsKey(Name))
                Progress[Name] = Value;
            else
            {
                Progress.Add(Name, Value);
            }
        }

        public void Load()
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists("PlayerData.dat"))
            {
                IsolatedStorageFileStream stream = isf.OpenFile("PlayerData.dat", System.IO.FileMode.Open);
                XmlReader reader = XmlReader.Create(stream);
                reader.ReadToFollowing("IsStored");
                IsStored = reader.ReadElementContentAsBoolean();
                if (IsStored)
                {
                    reader.ReadToFollowing("OGI");
                    OGI = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Hash");
                    Hash = reader.ReadElementContentAsString();
                }
                reader.ReadToFollowing("Difficulty");
                Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), reader.ReadElementContentAsString(), true);
                reader.ReadToFollowing("Points");
                Points = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("DeviceRegistered");
                DeviceRegistered = reader.ReadElementContentAsBoolean();
                reader.ReadToFollowing("ReportingEnabled");
                ReportingEnabled = reader.ReadElementContentAsBoolean();
                reader.ReadToFollowing("GameUsage");
                while (reader.Read())
                {
                    if (reader.IsStartElement("GameStat"))
                    {
                        VelesConflictReporting.GameUsageStastics statics = new VelesConflictReporting.GameUsageStastics();
                        reader.ReadToFollowing("Actions");
                        statics.Actions = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("Map");
                        statics.Map = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("Difficulty");
                        statics.Difficulty = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("Winner");
                        statics.Winner = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("TimeSpent");
                        statics.TimeSpent = new TimeSpan(reader.ReadElementContentAsLong());
                        statics.DeviceID = GameBase.DeviceID;
                        GameBase.GameUsage.Add(statics);
                    }
                    else if (reader.Name == "GameUsage")
                    {
                        break;
                    }

                }
                reader.ReadToFollowing("MenuUsage");
                while (reader.Read())
                {
                    if (reader.IsStartElement("MenuStat"))
                    {
                        VelesConflictReporting.MenuUsageStatistic statics = new VelesConflictReporting.MenuUsageStatistic();
                        reader.ReadToFollowing("Actions");
                        statics.Actions = reader.ReadElementContentAsInt();
                        reader.ReadToFollowing("Menu");
                        statics.Menu = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("TimeSpent");
                        statics.TimeSpent = new TimeSpan(reader.ReadElementContentAsLong());
                        statics.DeviceID = GameBase.DeviceID;
                        GameBase.MenuUsage.Add(statics);
                    }
                    else if (reader.Name == "MenuUsage")
                        break;
                }
                reader.ReadToFollowing("Research");
                reader.ReadToFollowing("Attack");
                Research["Attack"] = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("Defense");
                Research["Defense"] = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("Speed");
                Research["Speed"] = reader.ReadElementContentAsInt();
                reader.ReadToFollowing("Growth");
                Research["Growth"] = reader.ReadElementContentAsInt();



                while (reader.Read())
                {
                    if (reader.IsStartElement("ProgressEntry"))
                    {
                        reader.ReadToFollowing("Campaing");
                        string campaing = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("Progress");
                        int progress = reader.ReadElementContentAsInt();
                        Progress.Add(campaing, progress);
                    }
                }
                stream.Flush();
                stream.Dispose();
            }
            else
                FirstLoad = true;
            isf.Dispose();
        }
        public void Save()
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream stream = isf.OpenFile("PlayerData.dat", System.IO.FileMode.Create);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("PlayerData");
            writer.WriteElementString("IsStored", IsStored ? "true" : "false");
            if (IsStored)
            {
                writer.WriteElementString("OGI", OGI);
                writer.WriteElementString("Hash", Hash);
            }
            writer.WriteElementString("Difficulty", Difficulty.ToString());
            writer.WriteElementString("Points", Points.ToString());
            writer.WriteElementString("DeviceRegistered", DeviceRegistered ? "true" : "false");
            writer.WriteElementString("ReportingEnabled", ReportingEnabled ? "true" : "false");

            writer.WriteStartElement("GameUsage");
            lock (GameBase.GameUsage)
            {
                foreach (var v in GameBase.GameUsage)
                {
                    writer.WriteStartElement("GameStat");
                    writer.WriteElementString("Actions", v.Actions.ToString());
                    writer.WriteElementString("Map", v.Map);
                    writer.WriteElementString("Difficulty", v.Difficulty.ToString());
                    writer.WriteElementString("Winner", v.Winner.ToString());
                    writer.WriteElementString("TimeSpent", v.TimeSpent.Ticks.ToString());
                    writer.WriteEndElement();
                }
            }
            writer.WriteFullEndElement();
            writer.WriteStartElement("MenuUsage");
            lock (GameBase.MenuUsage)
            {
                foreach (var v in GameBase.MenuUsage)
                {
                    writer.WriteStartElement("MenuStat");
                    writer.WriteElementString("Actions", v.Actions.ToString());
                    writer.WriteElementString("Menu", v.Menu);
                    writer.WriteElementString("TimeSpent", v.TimeSpent.Ticks.ToString());
                    writer.WriteEndElement();
                }

            }
            writer.WriteFullEndElement();

            writer.WriteStartElement("Research");
            writer.WriteElementString("Attack", Research["Attack"].ToString());
            writer.WriteElementString("Defense", Research["Defense"].ToString());
            writer.WriteElementString("Speed", Research["Speed"].ToString());
            writer.WriteElementString("Growth", Research["Growth"].ToString());
            writer.WriteFullEndElement();

            writer.WriteStartElement("Entries");
            foreach (KeyValuePair<string, int> pairs in Progress)
            {
                writer.WriteStartElement("ProgressEntry");
                writer.WriteElementString("Campaing", pairs.Key);
                writer.WriteElementString("Progress", pairs.Value.ToString());
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();

            writer.WriteFullEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Dispose();
            stream.Dispose();
            isf.Dispose();
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameBase : Atom.Engine.Engine
    {
        internal static string Locale = "en";
        bool TutorialMode=true;
        internal static SpriteFont Font;
        MenuManager Manager;
        MenuManager PauseManager;
        IResourceObject<Texture2D> Background;
        Image GalaxyImage;

        Campaign selectedCampaing;
        Episode selectedEpisode;
        Mission selectedMission;
        internal static GameManager GameManager;
        List<Planet> GameSelectedPlanets = new List<Planet>();
        AtomScript GameScript;
        bool InternalPause = true;
        ContentManager LevelContentManager;
        Texture2D FleetTexture1;
        Texture2D FleetTexture2;
        AIManager AI;

        IInputServiceContract MainMenuContract;
        IInputServiceContract SinglePlayerContract;

        TextManager textManager;
        TextManagerSettings textSettings;
        SpriteSheetLoader SSL;
        IResourceCacheService RCS;
        MessageBox MessageBox;

        bool ShowEpilogueAfter = false;
        bool ShowStrategicAfter = false;
        MessageBox PopupMB;
        TextManager PopupTextManager;
        TextManagerSettings PopupTextSettings;

        internal static GameState GameState;
        Vector2 MissionOffset = Vector2.Zero;

        IAnimator<Vector2> ScienceAnimator = new SmoothVectorAnimator();
        IAnimator<Vector2> ScienceAnimatorReversed = new SmoothVectorAnimator();
        Vector2 SciencePosition = new Vector2(462, 8);
        Vector2 ScienceOrigin = new Vector2(2, 305);
        Rectangle ScienceRectangle = new Rectangle(0, 305, 50, 103);//new Rectangle(0, 305, 50, 103);
        IResourceObject<Texture2D> ScienceDNA;
        bool ScienceLock = false;
        ScienceState ScienceState = ScienceState.Forward;

        Camera camera;
        Vector2 cameraVelocity = Vector2.Zero;
        Vector2 offsetVelocity = Vector2.Zero;


        Texture2D ParalaxLayer1;
        Texture2D ParalaxLayer2;
        Texture2D ParalaxLayer3;

        internal static PlayerData playerData;
        int EpisodeProgressCounter = 0;
        bool Draging = false;
        private bool ScienceVisible;
        int GameActions;
        TimeSpan GameTimeSpent;
        internal static Texture2D Splash;
        IResourceObject<Texture2D> GameOverlay;

        Song soundTrack;
        SoundEffect MenuItemSelected, PlanetSelected, FleetSend, DoResearch;
        SoundEffectInstance PlanetSelectedInstance;

        ContentManager SoundContent;
        internal static List<VelesConflictReporting.GameUsageStastics> GameUsage = new List<VelesConflictReporting.GameUsageStastics>();
        internal static List<VelesConflictReporting.MenuUsageStatistic> MenuUsage = new List<VelesConflictReporting.MenuUsageStatistic>();
        internal static string DeviceID;
        bool UpdatingGameUsage = false;
        //SOMAWP7.SomaAd PublicAd;
        internal static AdManager adManager;
        internal static MultiplayerBase Multiplayer;
        int TutorialProgress = 0;
        HexFactory factory = new HexFactory();

        internal static Vector2 PlanetOrigin = new Vector2(106, 106);
        internal static Vector2 FleetOrigin = new Vector2(50, 50);

        RemoteManager remoteManager;
        public GameBase()
        {
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000 / 30f);
            GameState = GameState.Menu;
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            object Di = DeviceExtendedProperties.GetValue("DeviceUniqueId");
            DeviceID = GetSHA2((byte[])Di);
            playerData = new PlayerData();
            playerData.Load();
            playerData.ReportingEnabled = true;
            base.Initialize();
            base.OnUpdateStart += new Action<GameTime>(fOnUpdateStart);
            base.OnUpdateEnd += new Action<GameTime>(fOnUpdateEnd);
            base.OnDrawStart += new Action<GameTime>(fOnDrawStart);
            base.OnDrawEnd += new Action<GameTime>(fOnDrawEnd);

            GameManager = new GameManager();
            


            adManager.Initialize();
            TryPushStatistics();

            

            StartAgent();
        }

        private void StartAgent()
        {
            try
            {
                PeriodicTask periodicTask = ScheduledActionService.Find("NewsAgent") as PeriodicTask;
                // If the task already exists and background agents are enabled for the
                // application, you must remove the task and then add it again to update 
                // the schedule
                if (periodicTask != null)
                {
                    try
                    {
                        ScheduledActionService.Remove("NewsAgent");
                    }
                    catch
                    {
                    }
                }

                periodicTask = new PeriodicTask("NewsAgent");
                periodicTask.Description = "Queries news for Veles Conflict";

                // Place the call to Add in a try block in case the user has disabled agents.
                try
                {
                    ScheduledActionService.Add(periodicTask);
                    // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG)
                    ScheduledActionService.LaunchForTest("NewsAgent", TimeSpan.FromSeconds(10));
#endif
                }
                catch
                {

                }
            }
            catch
            {

            }
        }

        internal static MessageBox BuildMessageBox()
        {

            return BuildMessageBox(new Vector2(700,300));
        }

        internal static MessageBox BuildMessageBox(Vector2 dimensions)
        {
            IResourceCacheService RCS = Globals.Engine.GetService<IResourceCacheService>();
            MessageBox MessageBox = new MessageBox("", (int)dimensions.X, (int)dimensions.Y, Font, RCS.GetObject<Texture2D>("Corner"), RCS.GetObject<Texture2D>("Border"), RCS.GetObject<Texture2D>("BackgroundPattern"));
            MessageBox.Depth = 0.1f;
            MessageBox.Origin = new Vector2(dimensions.X/2, dimensions.Y/2);
            MessageBox.Position = new Vector2(400, 240);
            return MessageBox;
        }
        internal static string GetSHA2(byte[] bytes)
        {

            SHA256 sha2 = new SHA256Managed();

            byte[] result = sha2.ComputeHash(bytes);



            // Build the final string by converting each byte

            // into hex and appending it to a StringBuilder

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {

                sb.Append(result[i].ToString("X2"));

            }



            // And return it

            return sb.ToString();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Locale = LocalizationData.Locale;
            Locale = "ru";
            RCS = GetService(typeof(IResourceCacheService)) as IResourceCacheService;
            // Create a new SpriteBatch, which can be used to draw textures.
            factory.GetHexFill(410, new Vector2(512, 512));
            //Load splash screen
            Splash = Content.Load<Texture2D>(string.Format("Splash_{0}",Locale));
            spriteBatch.Begin();
            spriteBatch.Draw(Splash, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.Present();
            SoundContent = new ContentManager(this, "Content");
            soundTrack = SoundContent.Load<Song>(string.Format("Sound/Music/Song{0}", (DateTime.Now.Second % 5) + 1));
            MenuItemSelected = Content.Load<SoundEffect>("Sound\\Effects\\10");
            DoResearch = Content.Load<SoundEffect>("Sound\\Effects\\08");
            PlanetSelected = Content.Load<SoundEffect>("Sound\\Effects\\07");
            PlanetSelectedInstance = PlanetSelected.CreateInstance();
            FleetSend = Content.Load<SoundEffect>("Sound\\Effects\\08");
            
            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);
            if (MediaPlayer.GameHasControl)
            {
                MediaPlayer.Play(soundTrack);
            }


            SOMAWP7.SomaAd PublicAd = new SOMAWP7.SomaAd();
            PublicAd.Kws = "Space,Gods,Veles,Conflict,Windows,Phone,Cosmos,Astronomy,Sci-Fi,Russia";
            PublicAd.Adspace = 65766349;   // Developer Ads
            PublicAd.Pub = 923863405;       // Developer Ads
            PublicAd.AdSpaceHeight = 50;
            PublicAd.AdSpaceWidth = 300;
            
            //AdGameComponent.Initialize(this, "c8eda0b9-c5d9-4dc8-856b-8f0791cd3ddd");
            //AdGameComponent.Current.CountryOrRegion = System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
            //DrawableAd bannerAd = AdGameComponent.Current.CreateAd("10040438", new Rectangle(0, 0, 300, 50), true);
            
            adManager = new AdManager(PublicAd);

            RCS.PreCache<Texture2D>("Menues/Main/MainOverlay");


            #region Load Stuff

            GameOverlay = RCS.GetObject<Texture2D>("battle_03v");
            //
            //
            //
            //IResourceObject<Texture2D> StrategicOverlay = RCS.GetObject<Texture2D>("Menues/Strategic/Strategy");

            //RCS.PreCache<Texture2D>("selection");
            RCS.PreCache<Texture2D>("Menues/11v");
            RCS.PreCache<Texture2D>("Menues/LeftButton");
            RCS.PreCache<Texture2D>("Menues/RightButton");
            RCS.PreCache<Texture2D>("Menues/Base/bg_ext");
            Font = Content.Load<SpriteFont>("GameFont");
            IResourceObject<Texture2D> MainButton = RCS.GetObject<Texture2D>("Menues/Main/button");
            IResourceObject<Texture2D> CampaingButton = RCS.GetObject<Texture2D>("Menues/Campaing/SelectionButton");
            Campaign campaign1 = new Campaign();
            Campaign campaign2 = new Campaign();
            Campaign tutorial = new Campaign();
            using (XmlReader reader = XmlReader.Create(string.Format("Campaigns/Campaign1_{0}.xml", Locale)))
            {
                campaign1.Load(reader);
            }
            using (XmlReader reader = XmlReader.Create(string.Format("Campaigns/Campaign2_{0}.xml", Locale)))
            {
                campaign2.Load(reader);
            }
            using (XmlReader reader = XmlReader.Create(string.Format("Campaigns/Tutorial_{0}.xml", Locale)))
            {
                tutorial.Load(reader);
            }


            textSettings = new TextManagerSettings();
            textSettings.Font = Font;
            textSettings.Offset = new Vector2(425, 60);
            textSettings.Width = 310;
            textManager = new TextManager();
            textManager.Settings = textSettings;
            textManager.Text = campaign1.ShortDescription;
            textManager.Parse();

            selectedCampaing = tutorial;
            selectedEpisode = selectedCampaing.Episodes[0];
            selectedMission = selectedEpisode.Missions[0];
            #endregion
            #region Main Menu
            IResourceObject<Texture2D> MainOverlay = RCS.GetObject<Texture2D>("Menues/Main/MainOverlay");
            IAnimator<Vector2> MainAnimatorOut = new SmoothVectorAnimator();
            MainAnimatorOut.AtStart = Vector2.Zero;
            MainAnimatorOut.AtEnd = new Vector2(-120, 425);
            MainAnimatorOut.Duration = 450;

            IAnimator<Vector2> MainAnimatorIn = new LinearVectorAnimator();
            MainAnimatorIn.AtStart = new Vector2(120, -425);
            MainAnimatorIn.AtEnd = Vector2.Zero;
            MainAnimatorIn.Duration = 375;

            Manager = new MenuManager();
            Manager.ExitOnBack = true;

            Menu One = new Menu(MainAnimatorIn, MainAnimatorOut, BuildMessageBox());
            One.Controls.Add(new Image() { Texture = MainOverlay, Position = Vector2.Zero, Depth = 0.5f });
            Manager.Menues.Add("Main", One);
            bool Nab = playerData.FirstLoad || (playerData.GetProgress("Campaing1") == 1 && playerData.GetProgress("Campaing2") == 1 && playerData.GetProgress("Campaing3") == 1);
            One.Controls.Add(new AdControl(adManager) { Position = new Vector2(417, 411), Scale = 1, Depth = 0 });
            {
                if (Nab)
                {
                    HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
                    tb.Text = LocalizationData.NewGame;
                    tb.TextPosition = new Vector2(15, -47);
                    tb.Sound = MenuItemSelected.CreateInstance();
                    tb.UseSound = true;
                    tb.Font = Font;
                    tb.Depth = 0.9f;
                    tb.HoverColor = new Color(50, 238, 50);
                    tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                    tb.NextMenu = "Difficulty";
                    One.Controls.Add(tb);
                }
                else
                {
                    HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
                    tb.Text = LocalizationData.Continue;
                    tb.TextPosition = new Vector2(15, -47);
                    tb.Font = Font;
                    tb.Depth = 0.9f;
                    tb.Sound = MenuItemSelected.CreateInstance();
                    tb.HoverColor = new Color(50, 238, 50);
                    tb.UseSound = true;
                    tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                    tb.NextMenu = "Campaing";
                    One.Controls.Add(tb);
                }
            }
            {
                if (Nab)
                {
                    HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
                    tb.Text = LocalizationData.Continue;
                    tb.TextPosition = new Vector2(15, -47);
                    tb.Font = Font;
                    tb.Depth = 0.9f;
                    tb.Sound = MenuItemSelected.CreateInstance();
                    tb.HoverColor = new Color(50, 238, 50);
                    tb.UseSound = true;
                    tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                    tb.NextMenu = "Campaing";
                    One.Controls.Add(tb);
                }
                else
                {
                    HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
                    tb.Text = LocalizationData.NewGame;
                    tb.TextPosition = new Vector2(15, -47);
                    tb.Sound = MenuItemSelected.CreateInstance();
                    tb.HoverColor = new Color(50, 238, 50);
                    tb.UseSound = true;
                    tb.Font = Font;
                    tb.Depth = 0.9f;
                    tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                    tb.NextMenu = "Difficulty";
                    One.Controls.Add(tb);
                }

            }
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 2, 85 + 2 * 55));
                tb.Text = LocalizationData.Multiplayer;
                tb.TextPosition = new Vector2(15, -47);
                tb.NextMenu = "MultiplayerMain";
                tb.HoverColor = new Color(50, 238, 50);
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.UseSound = true;
                tb.Font = Font;
                tb.Depth = 0.9f;
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                One.Controls.Add(tb);
            }
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 3, 85 + 3 * 55));
                tb.Text = LocalizationData.Rate;
                tb.TextPosition = new Vector2(15, -47);
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.HoverColor = new Color(50, 238, 50);
                tb.UseSound = true;
                tb.Font = Font;
                tb.Depth = 0.9f;
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                tb.OnClick += new EventHandler(Rate_OnClick);
                One.Controls.Add(tb);
            }
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 4, 85 + 4 * 55));
                tb.Text = LocalizationData.Share;
                tb.TextPosition = new Vector2(15, -47);
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.HoverColor = new Color(50, 238, 50);
                tb.OnClick += new EventHandler(share_OnClick);
                tb.UseSound = true;
                tb.Font = Font;
                tb.Depth = 0.9f;
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                One.Controls.Add(tb);
            }
            /*
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 4, 85 + 4 * 55));
                tb.Text = "Editor";
                tb.OnClick+=new EventHandler(EnterEditorMode);
                tb.TextPosition = new Vector2(15, -47);
                tb.HoverColor = new Color(50, 238, 50);
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.UseSound = true;
                tb.Font = Font;
                tb.Depth = 0.9f;
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                One.Controls.Add(tb);
            }
            */
            {
                HybridButton tb = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 5, 85 + 5 * 55));
                tb.Text = LocalizationData.Exit;
                tb.TextPosition = new Vector2(15, -47);
                tb.HoverColor = new Color(50, 238, 50);
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.UseSound = true;
                tb.Font = Font;
                tb.Depth = 0.9f;
                tb.Origin = new Vector2(0, MainButton.Instance.Height - 7);
                tb.NextMenu = "%Exit%"; //Change
                One.Controls.Add(tb);
            }
            #endregion
            #region Campaing Menu
            RCS.PreCache<Texture2D>("Menues/Base/Base");
            RCS.PreCache<Texture2D>("Menues/Campaing/Overlay");
            IResourceObject<Texture2D> CampaingOverlay = RCS.GetObject<Texture2D>("Menues/Campaing/Overlay");
            IAnimator<Vector2> CampaingOut = new SmoothVectorAnimator();
            CampaingOut.AtStart = Vector2.Zero;
            CampaingOut.AtEnd = new Vector2(0, 480);
            CampaingOut.Duration = 500;

            IAnimator<Vector2> CampaingIn = new SmoothVectorAnimator();
            CampaingIn.AtStart = new Vector2(0, -480);
            CampaingIn.AtEnd = Vector2.Zero;
            CampaingIn.Duration = 500;
            Menu Two = new Menu(CampaingIn, CampaingOut, BuildMessageBox());
            Two.Controls.Add(new Image() { Texture = CampaingOverlay, Position = Vector2.Zero, Depth = 0.5f });
            Two.Controls.Add(new AdControl(adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
            Two.OnDraw += CampaignOnDraw;
            Manager.Menues.Add("Campaing", Two);
            {
                HybridButton tb = new HybridButton(CampaingButton, CampaingButton, new Vector2(57, 45 + 0 * 53));
                tb.Origin = new Vector2(0, 0);
                tb.Depth = 0.51f;
                tb.Color = Color.LightGreen;
                tb.HoverColor = new Color(50, 238, 50);
                tb.Font = Font;
                tb.UseSound = true;
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.Text = tutorial.Name;
                tb.Tag = tutorial;
                tb.OnClick += CampaignSelected;
                tb.TextPosition = new Vector2(20, CampaingButton.Instance.Height / 4);
                Two.Controls.Add(tb);
            }
            {
                HybridButton tb = new HybridButton(CampaingButton, CampaingButton, new Vector2(57, 45 + 1 * 53));
                tb.Origin = new Vector2(0, 0);
                tb.Depth = 0.51f;
                tb.Font = Font;
                tb.HoverColor = new Color(50, 238, 50);
                tb.UseSound = true;
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.Text = campaign1.Name;
                tb.Tag = campaign1;
                tb.OnClick += CampaignSelected;
                tb.TextPosition = new Vector2(20, CampaingButton.Instance.Height / 4);
                Two.Controls.Add(tb);
            }
            {
                HybridButton tb = new HybridButton(CampaingButton, CampaingButton, new Vector2(57, 45 + 2 * 53));
                tb.Origin = new Vector2(0, 0);
                tb.Depth = 0.51f;
                tb.Font = Font;
                tb.HoverColor = new Color(50, 238, 50);
                tb.UseSound = true;
                tb.Sound = MenuItemSelected.CreateInstance();
                tb.Text = campaign2.Name;
                tb.Tag = campaign2;
                tb.OnClick += CampaignSelected;
                tb.TextPosition = new Vector2(20, CampaingButton.Instance.Height / 4);
                Two.Controls.Add(tb);
            }
            //{
            //    HybridButton tb = new HybridButton(CampaingButton, CampaingButton, new Vector2(57, 45 + 1 * 53));
            //    tb.Origin = new Vector2(0, 0);
            //    tb.Depth = 0.51f;
            //    tb.Font = Font;
            //    tb.Text = campaign2.Name;
            //    tb.Tag = campaign2;
            //    tb.OnClick += CampaignSelected;
            //    tb.TextPosition = new Vector2(20, CampaingButton.Height / 4);
            //    Two.Controls.Add(tb);
            //}
            HybridButton Left = new HybridButton(RCS.GetObject<Texture2D>("Menues/LeftButton"), RCS.GetObject<Texture2D>("Menues/LeftButton"), new Vector2(43, 444));
            Left.Origin = new Vector2(0, 62);
            Left.NextMenu = "%Back%";
            Left.HoverColor = new Color(50, 238, 50);
            Left.CanOffset = false;
            Left.UseSound = true;
            Left.Sound = MenuItemSelected.CreateInstance();
            Left.Depth = 0.01f;
            Left.Font = Font;
            Left.TextPosition = -Left.Origin + new Vector2(50, 23);
            Left.Text = LocalizationData.Back;
            HybridButton Right = new HybridButton(RCS.GetObject<Texture2D>("Menues/RightButton"), RCS.GetObject<Texture2D>("Menues/RightButton"), new Vector2(757, 444));
            Right.Origin = new Vector2(172, 62);
            Right.CanOffset = false;
            Right.Depth = 0.01f;
            Right.HoverColor = new Color(50, 238, 50);
            Right.UseSound = true;
            Right.Sound = MenuItemSelected.CreateInstance();
            Right.NextMenu = "Base";
            Right.Font = Font;
            Right.TextPosition = -Right.Origin + new Vector2(60, 23);
            Right.Text = LocalizationData.Play;
            Two.Controls.Add(Left);
            Two.Controls.Add(Right);
            #endregion
            #region Base Menu

            IAnimator<Vector2> Empty = new SmoothVectorAnimator();
            Empty.AtStart = Vector2.Zero;
            Empty.AtEnd = Vector2.Zero;
            Empty.Duration = 0;

            #endregion

            MultiplayerMenu multiplayerMenu = new MultiplayerMenu();
            Manager.Menues.Add("MultiplayerMain", multiplayerMenu);

            //Define the strategic menu, but don't bother giving it any controls
            Menu Strategic = new Menu(Empty, Empty, BuildMessageBox());
            Manager.Menues.Add("Strategic", Strategic);
            Strategic.OnDraw += new EventHandler(Strategic_OnDraw);

            //Define the mission menu too
            Menu MissionSelection = new Menu(Empty, Empty, BuildMessageBox());
            Manager.Menues.Add("Mission", MissionSelection);
            MissionSelection.OnDraw += new EventHandler(MissionSelection_OnDraw);

            Menu DifficultySelection = new Menu(MainAnimatorIn, MainAnimatorOut, BuildMessageBox());
            DifficultySelection.NotReturnable = true;
            Manager.Menues.Add("Difficulty", DifficultySelection);
            /*
            HybridButton Easy = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
            Easy.Tag = "Easy";
            Easy.OnClick += new EventHandler(Easy_OnClick);
            Easy.Text = LocalizationData.DifficultyEasy;
            Easy.Font = Font;
            Easy.UseSound = true;
            Easy.Sound = MenuItemSelected.CreateInstance();
            Easy.Depth = 0.9f;
            Easy.NextMenu = "Campaing";
            Easy.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Easy.TextPosition = new Vector2(15, -47);
            */

            HybridButton Medium = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
            Medium.Tag = "Medium";
            Medium.OnClick += Easy_OnClick;
            Medium.UseSound = true;
            Medium.HoverColor = new Color(50, 238, 50);
            Medium.Sound = MenuItemSelected.CreateInstance();
            Medium.Text = LocalizationData.DifficultyEasy;
            Medium.NextMenu = "Campaing";
            Medium.Font = Font;
            Medium.Depth = 0.9f;
            Medium.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Medium.TextPosition = new Vector2(15, -47);

            HybridButton Hard = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
            Hard.Tag = "Hard";
            Hard.OnClick += Easy_OnClick;
            Hard.UseSound = true;
            Hard.HoverColor = new Color(50, 238, 50);
            Hard.Sound = MenuItemSelected.CreateInstance();
            Hard.Text = LocalizationData.DifficultyMedium;
            Hard.Font = Font;
            Hard.NextMenu = "Campaing";
            Hard.Depth = 0.9f;
            Hard.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Hard.TextPosition = new Vector2(15, -47);

            HybridButton Insane = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 2, 85 + 2 * 55));
            Insane.Tag = "Insane";
            Insane.OnClick += Easy_OnClick;
            Insane.Text = LocalizationData.DifficultyHard;
            Insane.UseSound = true;
            Insane.HoverColor = new Color(50, 238, 50);
            Insane.Sound = MenuItemSelected.CreateInstance();
            Insane.Font = Font;
            Insane.NextMenu = "Campaing";
            Insane.Depth = 0.9f;
            Insane.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Insane.TextPosition = new Vector2(15, -47);

            HybridButton DiffBack = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 3, 85 + 3 * 55));
            DiffBack.Text = LocalizationData.Back;
            DiffBack.Font = Font;
            DiffBack.HoverColor = new Color(50, 238, 50);
            DiffBack.UseSound = true;
            DiffBack.Sound = MenuItemSelected.CreateInstance();
            DiffBack.NextMenu = "%Back%";
            DiffBack.Depth = 0.9f;
            DiffBack.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            DiffBack.TextPosition = new Vector2(15, -47);
            DifficultySelection.Controls.Add(new AdControl(adManager) { Position = new Vector2(417, 411), Scale = 1, Depth = 0 });
            //DifficultySelection.Controls.Add(Easy);
            DifficultySelection.Controls.Add(Medium);
            DifficultySelection.Controls.Add(Hard);
            DifficultySelection.Controls.Add(Insane);
            DifficultySelection.Controls.Add(DiffBack);
            DifficultySelection.Controls.Add(new Image() { Texture = MainOverlay, Position = Vector2.Zero, Depth = 0.5f });

            PauseManager = new MenuManager();
            PauseManager.ExitOnBack = false;
            PauseManager.Enabled = false;
            Menu PauseMenu = new Menu(MainAnimatorIn, MainAnimatorOut, BuildMessageBox());
            PauseManager.Menues.Add("Pause", PauseMenu);
            PauseManager.SetMenu("Pause");
            HybridButton Continue = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
            Continue.UseSound = true;
            Continue.Sound = MenuItemSelected.CreateInstance();
            Continue.OnClick += new EventHandler(Continue_OnClick);
            Continue.Text = LocalizationData.Continue;
            Continue.Font = Font;
            Continue.HoverColor = new Color(50, 238, 50);
            Continue.Depth = 0.6f;
            Continue.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Continue.TextPosition = new Vector2(15, -47);
            PauseMenu.Controls.Add(new AdControl(adManager) { Position = new Vector2(417, 411), Scale = 1, Depth = 0 });

            HybridButton ChangeDifficulty = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
            ChangeDifficulty.UseSound = true;
            ChangeDifficulty.HoverColor = new Color(50, 238, 50);
            ChangeDifficulty.Sound = MenuItemSelected.CreateInstance();
            ChangeDifficulty.Text = LocalizationData.Difficulty;
            ChangeDifficulty.Font = Font;
            ChangeDifficulty.Depth = 0.6f;
            ChangeDifficulty.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            ChangeDifficulty.TextPosition = new Vector2(15, -47);
            ChangeDifficulty.NextMenu = "Difficulty";

            HybridButton Exit = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 2, 85 + 2 * 55));
            Exit.OnClick += new EventHandler(Exit_OnClick);
            Exit.HoverColor = new Color(50, 238, 50);
            Exit.UseSound = true;
            Exit.Sound = MenuItemSelected.CreateInstance();
            Exit.Text = LocalizationData.Exit;
            Exit.Font = Font;
            Exit.Depth = 0.6f;
            Exit.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            Exit.TextPosition = new Vector2(15, -47);
            PauseMenu.Controls.Add(Continue);
            PauseMenu.Controls.Add(Exit);
            PauseMenu.Controls.Add(ChangeDifficulty);
            PauseMenu.Controls.Add(new Image() { Texture = MainOverlay, Position = Vector2.Zero, Depth = 0.5f });

            Menu PauseMenuDifficulty = new Menu(MainAnimatorIn, MainAnimatorOut, BuildMessageBox());
            PauseManager.Menues.Add("Difficulty",PauseMenuDifficulty);
            /*
            HybridButton PauseMenuDifficultyEasy = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
            PauseMenuDifficultyEasy.Tag = "Easy";
            PauseMenuDifficultyEasy.OnClick += new EventHandler(PauseMenuDifficultyEasy_OnClick);
            PauseMenuDifficultyEasy.Text = LocalizationData.DifficultyEasy;
            PauseMenuDifficultyEasy.Font = Font;
            PauseMenuDifficultyEasy.UseSound = true;
            PauseMenuDifficultyEasy.Sound = MenuItemSelected.CreateInstance();
            PauseMenuDifficultyEasy.Depth = 0.9f;
            PauseMenuDifficultyEasy.NextMenu = "%Back%";
            PauseMenuDifficultyEasy.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            PauseMenuDifficultyEasy.TextPosition = new Vector2(15, -47);
            */

            HybridButton PauseMenuDifficultyMedium = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 0, 85 + 0 * 55));
            PauseMenuDifficultyMedium.Tag = "Medium";
            PauseMenuDifficultyMedium.HoverColor = new Color(50, 238, 50);
            PauseMenuDifficultyMedium.OnClick += new EventHandler(PauseMenuDifficultyEasy_OnClick);
            PauseMenuDifficultyMedium.UseSound = true;
            PauseMenuDifficultyMedium.Sound = MenuItemSelected.CreateInstance();
            PauseMenuDifficultyMedium.Text = LocalizationData.DifficultyEasy;
            PauseMenuDifficultyMedium.NextMenu = "%Back%";
            PauseMenuDifficultyMedium.Font = Font;
            PauseMenuDifficultyMedium.Depth = 0.9f;
            PauseMenuDifficultyMedium.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            PauseMenuDifficultyMedium.TextPosition = new Vector2(15, -47);

            HybridButton PauseMenuDifficultyHard = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 1, 85 + 1 * 55));
            PauseMenuDifficultyHard.Tag = "Hard";
            PauseMenuDifficultyHard.HoverColor = new Color(50, 238, 50);
            PauseMenuDifficultyHard.OnClick += new EventHandler(PauseMenuDifficultyEasy_OnClick);
            PauseMenuDifficultyHard.UseSound = true;
            PauseMenuDifficultyHard.Sound = MenuItemSelected.CreateInstance();
            PauseMenuDifficultyHard.Text = LocalizationData.DifficultyMedium;
            PauseMenuDifficultyHard.Font = Font;
            PauseMenuDifficultyHard.NextMenu = "%Back%";
            PauseMenuDifficultyHard.Depth = 0.9f;
            PauseMenuDifficultyHard.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            PauseMenuDifficultyHard.TextPosition = new Vector2(15, -47);

            HybridButton PauseMenuDifficultyInsane = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 2, 85 + 2 * 55));
            PauseMenuDifficultyInsane.Tag = "Insane";
            PauseMenuDifficultyInsane.HoverColor = new Color(50, 238, 50);
            PauseMenuDifficultyInsane.OnClick += new EventHandler(PauseMenuDifficultyEasy_OnClick);
            PauseMenuDifficultyInsane.Text = LocalizationData.DifficultyHard;
            PauseMenuDifficultyInsane.UseSound = true;
            PauseMenuDifficultyInsane.Sound = MenuItemSelected.CreateInstance();
            PauseMenuDifficultyInsane.Font = Font;
            PauseMenuDifficultyInsane.NextMenu = "%Back%";
            PauseMenuDifficultyInsane.Depth = 0.9f;
            PauseMenuDifficultyInsane.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            PauseMenuDifficultyInsane.TextPosition = new Vector2(15, -47);

            HybridButton PauseMenuDifficultyDiffBack = new HybridButton(MainButton, MainButton, new Vector2(120 - 15 * 3, 85 + 3 * 55));
            PauseMenuDifficultyDiffBack.Text = LocalizationData.Back;
            PauseMenuDifficultyDiffBack.Font = Font;
            PauseMenuDifficultyDiffBack.HoverColor = new Color(50, 238, 50);
            PauseMenuDifficultyDiffBack.UseSound = true;
            PauseMenuDifficultyDiffBack.Sound = MenuItemSelected.CreateInstance();
            PauseMenuDifficultyDiffBack.NextMenu = "%Back%";
            PauseMenuDifficultyDiffBack.Depth = 0.9f;
            PauseMenuDifficultyDiffBack.Origin = new Vector2(0, MainButton.Instance.Height - 7);
            PauseMenuDifficultyDiffBack.TextPosition = new Vector2(15, -47);
            PauseMenuDifficulty.Controls.Add(new AdControl(adManager) { Position = new Vector2(417, 411), Scale = 1, Depth = 0 });
            //PauseMenuDifficulty.Controls.Add(PauseMenuDifficultyEasy);
            PauseMenuDifficulty.Controls.Add(PauseMenuDifficultyMedium);
            PauseMenuDifficulty.Controls.Add(PauseMenuDifficultyHard);
            PauseMenuDifficulty.Controls.Add(PauseMenuDifficultyInsane);
            PauseMenuDifficulty.Controls.Add(PauseMenuDifficultyDiffBack);
            PauseMenuDifficulty.Controls.Add(new Image() { Texture = MainOverlay, Position = Vector2.Zero, Depth = 0.5f });

            Manager.SetMenu("Main");
            Manager.Enabled = true;

            PopupMB = new MessageBox("", 700, 300, Font, RCS.GetObject<Texture2D>("Corner"), RCS.GetObject<Texture2D>("Border"), RCS.GetObject<Texture2D>("BackgroundPattern"));
            PopupMB.Depth = 0.01f;
            PopupMB.OnClosed += new Action(PopupMB_OnClosed);
            PopupMB.Origin = new Vector2(350, 150);
            PopupMB.Position = new Vector2(400, 240);
            PopupTextManager = new TextManager();
            PopupTextManager.Settings = PopupTextSettings = new TextManagerSettings();
            PopupTextSettings.Depth = 0;
            PopupTextSettings.Font = Font;
            PopupTextSettings.Origin = new Vector2(400, 240);
            PopupTextManager.Settings.Offset = new Vector2(100 - 34, 105);
            PopupTextSettings.Width = 700 - 28;

            textSettings.Depth = 0.49f;
            IInputService input = Atom.Shared.Globals.Engine.GetService(typeof(IInputService)) as IInputService;
            MainMenuContract = input.CreateContract();
            MainMenuContract.SubscribedTouchEvents = TouchStates.OnDrag | TouchStates.OnDoubleTap | TouchStates.OnTap | TouchStates.OnFlick;
            MainMenuContract.TouchEventHandler += new EventHandler<TouchEventArgs>(contract_TouchEventHandler);
            SinglePlayerContract = input.CreateContract();
            SinglePlayerContract.Enabled = false;
            SinglePlayerContract.SubscribedTouchEvents = TouchStates.OnDrag | TouchStates.OnTap | TouchStates.OnDoubleTap | TouchStates.OnPinch | TouchStates.OnFlick;
            SinglePlayerContract.TouchEventHandler += new EventHandler<TouchEventArgs>(SinglePlayerContract_TouchEventHandler);

            Manager.OnEndMenuChange += new EventHandler<MenuChangeEventArgs>(Manager_OnEndMenuChange);
            Manager.OnBeginMenuChange += new EventHandler<MenuChangeEventArgs>(Manager_OnBeginMenuChange);
            Background = RCS.GetObject<Texture2D>("Menues/11v");
            foreach (KeyValuePair<string, Menu> menu in Manager.Menues)
            {
                menu.Value.InitializeControls();
            }
        }

        void EnterEditorMode(object sender, EventArgs e)
        {
            //Remote Begin
            remoteManager = new RemoteManager();
            remoteManager.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("192.168.1.109"), 29092));
            GameState = GameState.Editor;
            Manager.Enabled = false;
            MainMenuContract.Enabled = false;
            SetupCameraSmall();
            Width = 800;
            Height = 480;
            GameManager = new GameManager();
            LevelContentManager = new ContentManager(this, "Content");
            LevelContentManager.Load<Texture2D>("Graphics/Planets/Large/Populated/1");
        }

        internal void EditorStartGame()
        {
            InternalPause = false;
            Manager.Enabled = false;
            MainMenuContract.Enabled = false;
            SinglePlayerContract.Enabled = true;
            RCS.PreCache<Texture2D>("selection_planet_ingame");
            GameScript = new AtomScript(false, new Assembly[] { Assembly.GetExecutingAssembly() });
            GameScript.Pause = false;
            
            AI = new AIManager();
            FleetTexture1 = LevelContentManager.Load<Texture2D>("Graphics/Fleets/3");
            FleetTexture2 = LevelContentManager.Load<Texture2D>("Graphics/Fleets/5");
            Random rnd = new Random();
            int ParalaxFolder = rnd.Next(1, 6);
            ParalaxLayer1 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/1", ParalaxFolder));
            ParalaxLayer2 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/2", ParalaxFolder));
            ParalaxLayer3 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/3", ParalaxFolder));
            GameState = GameState.Singleplayer;
        }

        void PauseMenuDifficultyEasy_OnClick(object sender, EventArgs e)
        {
            playerData.Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), (sender as Control).Tag.ToString(), true);
        }

        

        void PopupMB_OnClosed()
        {
            if (ShowEpilogueAfter)
            {
                ShowEpilogueAfter = false;
                (Manager.Menues["Epilogue"] as EpilogueMenu).Prepare(selectedCampaing);
                Manager.SetMenu("Epilogue");
            }
            else if (ShowStrategicAfter)
            {
                ShowStrategicAfter = false;
                Manager.SetMenu("Strategic");
            }
        }


        void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped || MediaPlayer.State == MediaState.Paused)
            {
                Random rnd = new Random();
                SoundContent.Unload();
                soundTrack = SoundContent.Load<Song>(string.Format("Sound/Music/Song{0}", rnd.Next(1, 6)));
                //Soundtrack = Content.Load<Song>(String.Format("Sound\\SoundTrack{0}", rnd.Next(1, 4)));
                MediaPlayer.Play(soundTrack);
            }
        }





        protected override void OnExiting(object sender, EventArgs args)
        {
            if (playerData.ReportingEnabled)
            {
                foreach (KeyValuePair<string, Menu> menu in Manager.Menues)
                {
                    VelesConflict.VelesConflictReporting.MenuUsageStatistic mus = new VelesConflictReporting.MenuUsageStatistic();
                    mus.Actions = menu.Value.Actions;
                    mus.Menu = menu.Key;
                    mus.TimeSpent = menu.Value.TimeSpent;
                    mus.DeviceID = DeviceID;
                    lock (MenuUsage)
                    {
                        MenuUsage.Add(mus);
                    }
                }
            }
            playerData.Save();
            base.OnExiting(sender, args);
        }

        #region Menues On Draw

        void MissionSelection_OnDraw(object sender, EventArgs e)
        {
            textManager.Draw(spriteBatch);
            TexturedButton button = (from control in Manager.Menues["Mission"].Controls where control.Tag == selectedMission select control as TexturedButton).First();

        }
        void Strategic_OnDraw(object sender, EventArgs e)
        {
            textManager.Draw(SpriteBatch);
            spriteBatch.Draw(ScienceDNA.Instance, SciencePosition, ScienceRectangle, Color.White, 0f, ScienceOrigin - new Vector2(ScienceRectangle.X, ScienceRectangle.Y), 1f, SpriteEffects.None, 0.73f);
        }
        void CampaignOnDraw(object sender, EventArgs e)
        {

            textManager.Draw(spriteBatch);
        }

        void CampaignDescriptionOnDraw(object sender, EventArgs e)
        {
            textManager.Draw(spriteBatch);
        }
        #endregion
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(camera.Transforms));
        }
        bool CanDrag = true;
        #region TouchHandlers
        void SinglePlayerContract_TouchEventHandler(object sender, TouchEventArgs e)
        {

            Vector2 ActualLocation = ScreenToWorld(e.Location);
            Vector2 DragStart = ScreenToWorld(e.Location + e.Direction);

            if (MessageBox != null)
                if (MessageBox.Visible)
                {
                    if (MessageBox.Intersects(e.Location))
                    {
                        MessageBox.Close();
                    }
                }
            //if (InternalPause)
            //{
            //    if (e.State == TouchStates.OnTap)
            //    {
            //        if (MessageBox.Intersects(ActualLocation))
            //        {
            //            if (MessageBox.Visible)
            //            {
            //                MessageBox.Close();
            //                InternalPause = false;
            //                GameScript.Pause = false;
            //            }
            //        }
            //    }
            //    return;
            //}
            if (e.State != TouchStates.OnDrag)
                CanDrag = true;

            if (e.State == TouchStates.OnDrag || e.State == TouchStates.OnTap)
            {

                foreach (Planet planet in GameManager.State.PlayerPlanets)
                {
                    float Range = planet.PlanetSize * 64 + 32 * camera.Zoom;
                    if (e.State == TouchStates.OnDrag && CanDrag)
                    {
                        if (Vector2.Distance(planet.Position, DragStart) <= Range)
                        {
                            CanDrag = false;
                        }
                    }
                    if (Vector2.Distance(planet.Position, ActualLocation) <= Range)
                    {

                        if (!GameSelectedPlanets.Contains(planet))
                        {
                            GameActions++;
                            if (PlanetSelectedInstance.State != SoundState.Playing)
                            {
                                PlanetSelectedInstance.Play();
                            }
                            GameSelectedPlanets.Add(planet);
                        }
                    }

                }

                if (CanDrag)
                {
                    camera.Position -= e.Direction / camera.Zoom;
                    Vector2 cameraWorldMin = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.Transforms));
                    Vector2 cameraSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / camera.Zoom;
                    Vector2 limitWorldMin = new Vector2(0, 0);
                    Vector2 limitWorldMax = new Vector2(Width, Height);
                    Vector2 positionOffset = camera.Position - cameraWorldMin;
                    camera.Position = Vector2.Clamp(cameraWorldMin, limitWorldMin, limitWorldMax - cameraSize) + positionOffset;
                    //camera.Position = Vector2.Clamp(camera.Position, new Vector2(400, 240), new Vector2(Width-400, Height-240));

                }
            }
            else if (e.State == TouchStates.OnDoubleTap)
            {
                foreach (Planet planet in GameManager.State.Planets)
                {
                    GameActions++;
                    float Range = planet.PlanetSize * 64 + 32;
                    if (Vector2.Distance(planet.Position, ActualLocation) <= Range)
                    {
                        foreach (Planet selected in GameSelectedPlanets)
                            GameManager.SendFleet(selected.Forces / 2, selected, planet);
                    }
                }
                FleetSend.Play();
                GameSelectedPlanets.Clear();
            }
            else if (e.State == TouchStates.OnPinch)
            {
                if (Width > 800 && Height > 480)
                {
                    Vector2 Old1 = e.Location - e.Direction;
                    Vector2 Old2 = e.Location2 - e.Direction2;
                    float newDistance = Vector2.Distance(e.Location, e.Location2);
                    float oldDistance = Vector2.Distance(Old1, Old2);
                    float Factor = newDistance / oldDistance;
                    camera.Zoom *= Factor;
                    float minZoomX = (float)GraphicsDevice.Viewport.Width / Width;
                    float minZoomY = (float)GraphicsDevice.Viewport.Height / Height;
                    camera.Zoom = MathHelper.Clamp(camera.Zoom, MathHelper.Max(minZoomX, minZoomY), 1f);
                    Vector2 cameraWorldMin = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.Transforms));
                    Vector2 cameraSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / camera.Zoom;
                    Vector2 limitWorldMin = new Vector2(0, 0);
                    Vector2 limitWorldMax = new Vector2(Width, Height);
                    Vector2 positionOffset = camera.Position - cameraWorldMin;
                    camera.Position = Vector2.Clamp(cameraWorldMin, limitWorldMin, limitWorldMax - cameraSize) + positionOffset;
                }
            }
            else if (e.State == TouchStates.OnFlick && CanDrag)
            {
                cameraVelocity = -e.Direction / 100;
            }


        }

        void contract_TouchEventHandler(object sender, TouchEventArgs e)
        {
            if (Manager.SelectedMenu == "Exit")
            {
                if (e.State == TouchStates.OnDoubleTap)
                    Exit();
                return;
            }

            if (e.State == TouchStates.OnFlick)
            {
                offsetVelocity = e.Direction / 100;
                return;
            }
            #region Process Menu Input
            if (Manager.SelectedMenu == "Base" && textManager.Height > 320 && e.State == TouchStates.OnDrag)
            {
                textSettings.Offset = Vector2.Clamp(textSettings.Offset + new Vector2(0, e.Direction.Y), new Vector2(67, 54 - (textManager.Height - 320)), new Vector2(67, 54));
            }
            else if (Manager.SelectedMenu == "Epilogue" && e.State == TouchStates.OnDrag)
            {
                (Manager.Menues["Epilogue"] as EpilogueMenu).MoveText(e);
            }
            else if (Manager.SelectedMenu == "Strategic")
            {
                Rectangle intRect = new Rectangle((int)e.Location.X, (int)e.Location.Y, 5, 5);
                Rectangle intLoc = new Rectangle((int)SciencePosition.X, (int)SciencePosition.Y, ScienceRectangle.Width, ScienceRectangle.Height);
                Draging = false;
                if (intRect.Intersects(intLoc))
                {

                    if (e.State == TouchStates.OnDrag && ScienceAnimator.State == AnimatorState.Stoped && ScienceAnimatorReversed.State == AnimatorState.Stoped)
                    {
                        Draging = true;
                        SciencePosition += e.Direction;
                        if (SciencePosition.X < 27)
                            SciencePosition.X = 27;
                        else if (SciencePosition.X > 462)
                            SciencePosition.X = 462;
                        if (SciencePosition.Y < 8)
                            SciencePosition.Y = 8;
                        else if (SciencePosition.Y > 319)
                            SciencePosition.Y = 319;

                        ScienceRectangle.Width = 512 - (int)SciencePosition.X;
                        ScienceRectangle.Height = (int)SciencePosition.Y - 8 + 103;
                        ScienceRectangle.Y = 305 - ((int)SciencePosition.Y - 8);


                    }
                }
                else if (!ScienceLock)
                {
                    GalaxyImage.Offset = Vector2.Clamp(GalaxyImage.Offset + e.Direction * 0.5f, new Vector2(-224 - 310, -480), new Vector2(0, 0));
                    foreach (Control control in Manager.Menues["Strategic"].Controls)
                    {
                        HexControl ab = control as HexControl;
                        if (ab != null)
                        {
                            ab.Offset = GalaxyImage.Offset;
                        }
                    }
                }
            }
            else if (Manager.SelectedMenu == "Mission")
            {
                if (Manager.Menues["Mission"].Blocking)
                    return;

                MissionOffset = Vector2.Clamp(MissionOffset + new Vector2(e.Direction.X, 0), new Vector2(-(Manager.Menues["Mission"].Controls.Count - 3) * 240 + 400, 0), new Vector2(0, 0));
                foreach (Control control in Manager.Menues["Mission"].Controls)
                {
                    TexturedButton ab = control as TexturedButton;
                    if (ab != null)
                    {
                        ab.Offset = MissionOffset;
                    }
                }

            }
            #endregion
        }

        #endregion
        #region On Click
        TexturedButton selectedResearch = null;
        private int Width;
        private int Height;
        void share_OnClick(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.ShareLinkTask shareLinkTask = new Microsoft.Phone.Tasks.ShareLinkTask();
                shareLinkTask.Title = "Veles Conflict";
                shareLinkTask.LinkUri = new Uri("http://www.velesconflict.com", UriKind.Absolute);
                shareLinkTask.Message = LocalizationData.ShareText;
                shareLinkTask.Show();
            }
            catch
            {
            }
        }
        void Rate_OnClick(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.MarketplaceReviewTask mrt = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
                mrt.Show();
            }
            catch (Exception)
            {
            }
        }
        void Exit_OnClick(object sender, EventArgs e)
        {
            Manager.Enabled = true;
            MainMenuContract.Enabled = true;
            SinglePlayerContract.Enabled = false;
            GameScript.Dispose();
            GameScript = null;
            GameState = GameState.Menu;
            LevelContentManager.Unload();
            LevelContentManager.Dispose();
            PauseManager.Enabled = false;
            //textSettings.Scale = 1;
            //textSettings.Origin = Vector2.Zero;
            //textSettings.Width = 200;
            //textSettings.Offset = new Vector2(545, 50);
            //textSettings.Depth = 0.49f;
            textManager.Settings.Width = 200;
            textManager.Settings.Offset = new Vector2(545, 50);
            textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
            textManager.Parse();
            GameManager = null;
            GameActions = 0;
            GameTimeSpent = TimeSpan.Zero;
        }
        void Continue_OnClick(object sender, EventArgs e)
        {
            PauseManager.Enabled = false;
            InternalPause = false;
            GameScript.Pause = false;
            GameState = GameState.Singleplayer;

        }
        void Easy_OnClick(object sender, EventArgs e)
        {
            playerData = new PlayerData(playerData);
            playerData.Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), (sender as Control).Tag.ToString(), true);
            playerData.Save();
        }
        void RightMission_OnClick(object sender, EventArgs e)
        {
            GameSelectedPlanets.Clear();
            textSettings.Width = 700 - 28;
            textManager.Text = selectedMission.Description;
            textManager.Parse();
            InternalPause = false;
            Manager.Enabled = false;
            MainMenuContract.Enabled = false;
            SinglePlayerContract.Enabled = true;
            RCS.PreCache<Texture2D>("selection_planet_ingame");
            LoadMission(selectedMission);
            if (TutorialMode)
            {
                if (TutorialProgress == 5)
                {
                    MessageBox = new MessageBox("", 700, 300, Font, RCS.GetObject<Texture2D>("Corner"), RCS.GetObject<Texture2D>("Border"), RCS.GetObject<Texture2D>("BackgroundPattern"));
                    MessageBox.Depth = 0.1f;
                    MessageBox.Show(false);
                    MessageBox.Origin = new Vector2(350, 150);
                    MessageBox.Position = new Vector2(Width / 2, Height / 2);
                    MessageBox.Text = LocalizationData.Tutorial5;
                }
            }
            else
                MessageBox = null;
            GameState = GameState.Singleplayer;
            AI = new AIManager();
        }
        void tb_OnClick(object sender, EventArgs e)
        {
            TexturedButton button = (from control in Manager.Menues["Mission"].Controls where control.Tag == selectedMission select control as TexturedButton).First();
            button.Color = Color.White;
            button = sender as TexturedButton;
            button.Color = Color.Gray;
            Mission mission = (sender as Control).Tag as Mission;
            selectedMission = mission;
            textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
            textManager.Parse();
        }
        void CampaignSelected(object sender, EventArgs e)
        {
            Control c = (from control in Manager.Menues["Campaing"].Controls where control.Tag == selectedCampaing select control).FirstOrDefault();
            if (c != null)
            {
                c.Color = Color.White;
            }

            HybridButton button = sender as HybridButton;
            button.Color = Color.LightGreen;
            selectedCampaing = (button.Tag as Campaign);
            selectedEpisode = selectedCampaing.Episodes[0];
            selectedMission = selectedEpisode.Missions[0];
            TutorialMode = selectedCampaing.InternalName == "Tutorial";
            TutorialProgress = 0;
            textManager.Text = selectedCampaing.Name + "\n\n\n" + selectedCampaing.ShortDescription;
            textManager.Parse();
        }

        void hexControl_OnClick(Cell obj)
        {

            if (obj.Color != Color.Yellow * 0.5f)
            {
                return;
            }
            Episode episode = obj.Tag as Episode;
            EpisodeProgressCounter = 0;
            foreach (Episode ep in selectedCampaing.Episodes)
            {
                if (ep != episode)
                    EpisodeProgressCounter += ep.Missions.Count;
                else
                    break;
            }
            selectedEpisode = episode;
            textManager.Text = episode.Name + "\n\n\n\n\n" + episode.Description;
            textManager.Parse();
        }
        void ScienceOnClick(object sender, EventArgs e)
        {
            TexturedButton control = sender as TexturedButton;
            selectedResearch = control;
            string s = control.Tag.ToString();
            switch (s)
            {

                case "Science1":
                    textManager.Text = string.Format(LocalizationData.Science1 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Growth"] + 1, playerData.Points);
                    textManager.Parse();
                    break;
                case "Science2":
                    textManager.Text = string.Format(LocalizationData.Science2 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Speed"] + 1, playerData.Points);
                    textManager.Parse();
                    break;
                case "Science3":
                    textManager.Text = string.Format(LocalizationData.Science3 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Attack"] + 1, playerData.Points);
                    textManager.Parse();
                    break;
                case "Science4":
                    textManager.Text = string.Format(LocalizationData.Science4 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Defense"] + 1, playerData.Points);
                    textManager.Parse();
                    break;
            }
        }
        void hb_OnClick(object sender, EventArgs e)
        {
            if (selectedResearch != null)
            {
                string s = selectedResearch.Tag.ToString();
                bool LockScience = false;
                int Cost = 1;
                foreach (KeyValuePair<string,int> data in playerData.Research)
                {
                    if (data.Value >= 5)
                        LockScience = true;

                }
                switch (s)
                {
                    case "Science1":
                        if (playerData.Research["Growth"] >= 4 && LockScience)
                        {
                            break;
                        }
                        Cost = playerData.Research["Growth"] + 1;
                        if (Cost <= playerData.Points)
                            playerData.Points -= Cost;
                        else
                            break;
                        playerData.Research["Growth"]++;
                        {
                            int TextureNumber = playerData.Research["Growth"] + 1;
                            if (TextureNumber > 6)
                                TextureNumber = 6;
                            playerData.Research["Growth"] = TextureNumber - 1;
                            selectedResearch.Normal = RCS.GetObject<Texture2D>(string.Format("Science/1_{0}", TextureNumber));
                            selectedResearch.Hovering = RCS.GetObject<Texture2D>(string.Format("Science/1_{0}", TextureNumber));
                        }
                        textManager.Text = string.Format(LocalizationData.Science1 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Growth"] + 1, playerData.Points);
                        textManager.Parse();
                        break;
                    case "Science2":
                        if (playerData.Research["Speed"] >=4 && LockScience)
                        {
                            break;
                        }
                        Cost = playerData.Research["Speed"] + 1;
                        if (Cost <= playerData.Points)
                            playerData.Points -= Cost;
                        else
                            break;
                        playerData.Research["Speed"]++;
                        {
                            int TextureNumber = playerData.Research["Speed"] + 1;
                            if (TextureNumber > 6)
                                TextureNumber = 6;
                            playerData.Research["Speed"] = TextureNumber - 1;
                            selectedResearch.Normal = RCS.GetObject<Texture2D>(string.Format("Science/2_{0}", TextureNumber));
                            selectedResearch.Hovering = RCS.GetObject<Texture2D>(string.Format("Science/2_{0}", TextureNumber));
                        }
                        textManager.Text = string.Format(LocalizationData.Science2 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Speed"] + 1, playerData.Points);
                        textManager.Parse();
                        break;
                    case "Science3":
                        if (playerData.Research["Attack"] >= 4 && LockScience)
                        {
                            break;
                        }
                        Cost = playerData.Research["Attack"] + 1;
                        if (Cost <= playerData.Points)
                            playerData.Points -= Cost;
                        else
                            break;
                        playerData.Research["Attack"]++;
                        {
                            int TextureNumber = playerData.Research["Attack"] + 1;
                            if (TextureNumber > 6)
                                TextureNumber = 6;
                            playerData.Research["Attack"] = TextureNumber - 1;
                            selectedResearch.Normal = RCS.GetObject<Texture2D>(string.Format("Science/3_{0}", TextureNumber));
                            selectedResearch.Hovering = RCS.GetObject<Texture2D>(string.Format("Science/3_{0}", TextureNumber));
                        }
                        textManager.Text = string.Format(LocalizationData.Science3 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Attack"] + 1, playerData.Points);
                        textManager.Parse();
                        break;
                    case "Science4":

                        if (playerData.Research["Defense"] >=4 && LockScience)
                        {
                            break;
                        }
                        Cost = playerData.Research["Defense"] + 1;
                        if (Cost <= playerData.Points)
                            playerData.Points -= Cost;
                        else
                            break;
                        playerData.Research["Defense"]++;
                        {
                            int TextureNumber = playerData.Research["Defense"] + 1;
                            if (TextureNumber > 6)
                                TextureNumber = 6;
                            playerData.Research["Defense"] = TextureNumber - 1;
                            selectedResearch.Normal = RCS.GetObject<Texture2D>(string.Format("Science/4_{0}", TextureNumber));
                            selectedResearch.Hovering = RCS.GetObject<Texture2D>(string.Format("Science/4_{0}", TextureNumber));
                        }
                        textManager.Text = string.Format(LocalizationData.Science4 + "\n" + LocalizationData.AvailablePoints + ":{1}", playerData.Research["Defense"] + 1, playerData.Points);
                        textManager.Parse();
                        break;
                }

            }
            try
            {
                //foreach (KeyValuePair<string, int> pair in playerData.Research)
                //{
                //    if (playerData.Research[pair.Key] > 6)
                //       playerData.Research[pair.Key] = 6;
                //}
            }
            catch
            {

            }
        }
        #endregion

        #region Statistics
        void TryPushStatistics()
        {
            if (playerData.ReportingEnabled)
            {
                if (DeviceNetworkInformation.IsNetworkAvailable)
                {
                    VelesConflictReporting.ReportingServiceClient rsc = new VelesConflictReporting.ReportingServiceClient();
                    rsc.RegisterDeviceCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(rsc_RegisterDeviceCompleted);

                    if (!playerData.DeviceRegistered)
                    {
                        rsc.RegisterDeviceAsync(DeviceID, rsc);
                    }
                    else
                    {
                        if (GameUsage.Count > 0)
                        {
                            lock (GameUsage)
                            {
                                UpdatingGameUsage = true;
                                rsc.AddBatchGameUsageStasticsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(rsc_AddBatchGameUsageStasticsCompleted);
                                rsc.AddBatchGameUsageStasticsAsync(GameUsage.ToArray(), GameUsage.ToArray());
                            }
                        }
                        if (MenuUsage.Count > 0)
                        {
                            lock (MenuUsage)
                            {
                                rsc.AddBatchMenuUsageStasticsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(rsc_AddBatchMenuUsageStasticsCompleted);
                                rsc.AddBatchMenuUsageStasticsAsync(MenuUsage.ToArray(), MenuUsage.ToArray());
                            }
                        }
                        rsc.CloseAsync();
                    }
                }
            }
        }

        void rsc_RegisterDeviceCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                playerData.DeviceRegistered = true;
                VelesConflictReporting.ReportingServiceClient rsc = e.UserState as VelesConflictReporting.ReportingServiceClient;
                if (GameUsage.Count > 0)
                {
                    lock (GameUsage)
                    {
                        rsc.AddBatchGameUsageStasticsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(rsc_AddBatchGameUsageStasticsCompleted);
                        rsc.AddBatchGameUsageStasticsAsync(GameUsage.ToArray(), GameUsage.ToArray());
                    }
                }
                if (MenuUsage.Count > 0)
                {
                    lock (MenuUsage)
                    {
                        rsc.AddBatchMenuUsageStasticsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(rsc_AddBatchMenuUsageStasticsCompleted);
                        rsc.AddBatchMenuUsageStasticsAsync(MenuUsage.ToArray(), MenuUsage.ToArray());
                    }
                }
                rsc.CloseAsync();
            }
        }

        void rsc_AddBatchMenuUsageStasticsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                lock (MenuUsage)
                {
                    VelesConflictReporting.MenuUsageStatistic[] s = (VelesConflictReporting.MenuUsageStatistic[])e.UserState;
                    foreach (VelesConflictReporting.MenuUsageStatistic stat in s)
                    {
                        MenuUsage.Remove(stat);
                    }
                }
            }

        }

        void rsc_AddBatchGameUsageStasticsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                lock (GameUsage)
                {

                    VelesConflictReporting.GameUsageStastics[] s = (VelesConflictReporting.GameUsageStastics[])e.UserState;
                    foreach (VelesConflictReporting.GameUsageStastics stat in s)
                    {
                        GameUsage.Remove(stat);
                    }
                }
            }
            UpdatingGameUsage = false;
        }
        #endregion

        
        void LoadMission(Mission mission)
        {
            GameActions = 0;
            GameTimeSpent = TimeSpan.Zero;
            spriteBatch.Begin();
            spriteBatch.Draw(Splash, Vector2.Zero, Color.White);
            spriteBatch.End();
            GraphicsDevice.Present();
            RCS.Release(TimeSpan.Zero);
            LevelContentManager = new ContentManager(this, "Content");
            GameManager = new GameManager();
            GameScript = new AtomScript(false, new Assembly[] { Assembly.GetExecutingAssembly() });
            GameScript.Pause = false;
            string map = "";
            Random rnd = new Random();
            //Properly resolve the mission address
            if (mission.Map.StartsWith("XAP\\"))
            {
                //internal content
                map = mission.Map.Remove(0, 4);
            }

            int LargeNotPopulated = 3;
            int LargePopulated = 4;

            int MediumNotPopulated = 2;
            int MediumPopulated = 3;

            int SmallNotPopulated = 3;
            int SmallPopulated = 3;

            List<Planet> mapPlanets = new List<Planet>();
            XmlReader xmlReader = XmlReader.Create(map);
            xmlReader.ReadStartElement();

            xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("Width");
            Width = xmlReader.ReadContentAsInt();

            xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("Height");
            Height = xmlReader.ReadContentAsInt();

            if (Width > 800 && Height > 480)
            {
                SetupCameraBig();
            }
            else
            {
                SetupCameraSmall();
            }

            xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("Format");
            int Format = xmlReader.ReadContentAsInt();

            xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("HasScript");
            bool HasScript = xmlReader.ReadContentAsBoolean();
            if (HasScript)
            {
                xmlReader.ReadToFollowing("Script");
                GameScript.Load(xmlReader);
            }
            #region Load Map
            while (xmlReader.Read())
            {
                /*Read planet header*/
                if (xmlReader.LocalName == "Planet" && xmlReader.IsStartElement())
                {
                    /*Read the ID element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("ID");
                    int ID = xmlReader.ReadContentAsInt();

                    /*Read the owner element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Owner");
                    string owner = xmlReader.ReadContentAsString();

                    /*Read the forces element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Forces");
                    int forces = xmlReader.ReadContentAsInt();

                    /*Read the growth element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Growth");
                    int growth = xmlReader.ReadContentAsInt();

                    /*Read the growth cooldown element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("GrowthCooldown");
                    int growthcd = xmlReader.ReadContentAsInt();

                    /*Read the size element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Size");
                    float size = xmlReader.ReadContentAsFloat();

                    /*Read the people element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("HasPeople");
                    bool hasppl = xmlReader.ReadContentAsBoolean();

                    /*Read the Position element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Position");
                    Microsoft.Xna.Framework.Vector2 Position = new Microsoft.Xna.Framework.Vector2();

                    /*Read the X element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("X");
                    Position.X = xmlReader.ReadContentAsInt();

                    /*Read the Y element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Y");
                    Position.Y = xmlReader.ReadContentAsInt();

                    Planet p = new Planet();
                    p.Id = ID;
                    p.Position = Position;
                    p.Growth = growth;
                    p.GrowthCounter = growthcd;
                    p.GrowthReset = growthcd;
                    p.Owner = (PlayerType)Enum.Parse(typeof(PlayerType), owner, false);
                    p.Forces = forces;
                    p.PlanetSize = size;
                    p.HasPeople = hasppl;

                    PlanetSize pz = PlanetSize.Large;
                    //Determine the load location
                    string TextureLocation = "Graphics/Planets/";
                    if (p.PlanetSize >= 0.6)
                    {
                        pz = PlanetSize.Large;
                    }
                    else
                    {
                        if(rnd.NextDouble()<0.35)
                            pz=PlanetSize.Small;
                        else
                            pz=PlanetSize.Medium;
                    }

                    TextureLocation += pz.ToString() + "/";
                    switch (pz)
                    {
                        case PlanetSize.Large:
                            if (p.HasPeople)
                            {
                                int Number = rnd.Next(1, (LargePopulated + 1));
                                TextureLocation += "Populated/" + Number.ToString();
                            }
                            else
                            {
                                int Number = rnd.Next(1, (LargeNotPopulated + 1));
                                TextureLocation += "NotPopulated/" + Number.ToString();
                            }
                            break;
                        case PlanetSize.Medium:
                            if (p.HasPeople)
                            {
                                int Number = rnd.Next(1, (MediumPopulated + 1));
                                TextureLocation += "Populated/" + Number.ToString();
                            }
                            else
                            {
                                int Number = rnd.Next(1, (MediumNotPopulated + 1));
                                TextureLocation += "NotPopulated/" + Number.ToString();
                            }
                            break;
                        case PlanetSize.Small:
                            if (p.HasPeople)
                            {
                                int Number = rnd.Next(1, (SmallPopulated + 1));
                                TextureLocation += "Populated/" + Number.ToString();
                            }
                            else
                            {
                                int Number = rnd.Next(1, (SmallNotPopulated + 1));
                                TextureLocation += "NotPopulated/" + Number.ToString();
                            }
                            break;
                    }


                    p.Texture = LevelContentManager.Load<Texture2D>(TextureLocation);
                    mapPlanets.Add(p);
                }

            }
            #endregion
            FleetTexture1 = LevelContentManager.Load<Texture2D>("Graphics/Fleets/3");
            FleetTexture2 = LevelContentManager.Load<Texture2D>("Graphics/Fleets/5");
            int ParalaxFolder = rnd.Next(1, 6);
            ParalaxLayer1 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/1", ParalaxFolder));
            ParalaxLayer2 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/2", ParalaxFolder));
            ParalaxLayer3 = LevelContentManager.Load<Texture2D>(string.Format("Graphics/Backgrounds/{0}/3", ParalaxFolder));
            GameManager.State.Planets.AddRange(mapPlanets);
            GameScript.LoadScript();
            GameScript.Pause = false;
            xmlReader.Dispose();
        }
        void Manager_OnBeginMenuChange(object sender, MenuChangeEventArgs e)
        {
            offsetVelocity = Vector2.Zero;
            if (e.Menu == Manager.Menues["Campaing"])
            {
                selectedEpisode = selectedCampaing.Episodes[0];
                selectedMission = selectedEpisode.Missions[0];
                TutorialMode = selectedCampaing.InternalName == "Tutorial";
                TutorialProgress = 0;
                textManager.Text = selectedCampaing.Name + "\n\n\n" + selectedCampaing.ShortDescription;
                textManager.Parse();

                RCS.PreCache<Texture2D>("Menues/Strategic/Strategy");
                RCS.PreCache<Texture2D>("Menues/Strategic/06");
                RCS.PreCache<Texture2D>("Menues/Strategic/Hex2");
                RCS.PreCache<Texture2D>("Menues/Strategic/Screen");
                RCS.PreCache<Texture2D>("Menues/Strategic/science_screen");


                for (int i = 1; i <= 4; i++)
                {
                    //Load science
                    for (int j = 1; j <= 6; j++)
                    {
                        RCS.PreCache<Texture2D>(string.Format("Science/{0}_{1}", i, j));
                    }
                }
                RCS.PreCache<Texture2D>("Menues/Strategic/selection_tactical");
                textSettings.Width = 310;
                textSettings.Offset = new Vector2(425, 60);
                textManager.Text = selectedCampaing.Name + "\n\n\n" + selectedCampaing.ShortDescription;
                textManager.Parse();
                RCS.PreCache<Texture2D>("Menues/Strategic/06");
                if (SSL == null)
                {
                    SSL = new SpriteSheetLoader();

                    ScienceAnimator.AtStart = new Vector2(462, 8);
                    ScienceAnimator.AtEnd = new Vector2(25, 319);
                    ScienceAnimator.Duration = 1000;

                    ScienceAnimatorReversed.AtEnd = new Vector2(462, 8);
                    ScienceAnimatorReversed.AtStart = new Vector2(25, 319);
                    ScienceAnimatorReversed.Duration = 1000;

                    IAnimator<Vector2> Empty = new SmoothVectorAnimator();
                    Empty.AtStart = Vector2.Zero;
                    Empty.AtEnd = Vector2.Zero;
                    Empty.Duration = 0;
                    IResourceObject<Texture2D> BaseOverlay = RCS.GetObject<Texture2D>("Menues/Base/Base");
                    Menu Three = new Menu(Empty, Empty, BuildMessageBox());
                    Three.OnDraw += CampaignDescriptionOnDraw;
                    Manager.Menues.Add("Base", Three);
                    Three.Controls.Add(new Image() { Texture = BaseOverlay, Position = Vector2.Zero, Depth = 0.48f });
                    Three.Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Base/bg_ext"), Position = new Vector2(0, 0), Depth = 0.6f });
                    Three.Controls.Add(new AdControl(adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
                    HybridButton LeftBase = new HybridButton(RCS.GetObject<Texture2D>("Menues/LeftButton"), RCS.GetObject<Texture2D>("Menues/LeftButton"), new Vector2(43, 444));
                    LeftBase.Origin = new Vector2(0, 62);
                    LeftBase.NextMenu = "%Back%";
                    LeftBase.CanOffset = false;
                    LeftBase.Depth = 0.01f;
                    LeftBase.Font = Font;
                    LeftBase.HoverColor = new Color(50, 238, 50);
                    LeftBase.UseSound = true;
                    LeftBase.Sound = MenuItemSelected.CreateInstance();
                    LeftBase.TextPosition = -LeftBase.Origin + new Vector2(50, 23);
                    LeftBase.Text = LocalizationData.Back;
                    HybridButton RightBase = new HybridButton(RCS.GetObject<Texture2D>("Menues/RightButton"), RCS.GetObject<Texture2D>("Menues/RightButton"), new Vector2(757, 444));
                    RightBase.Origin = new Vector2(172, 62);
                    RightBase.CanOffset = false;
                    RightBase.Depth = 0.01f;
                    RightBase.HoverColor = new Color(50, 238, 50);
                    RightBase.UseSound = true;
                    RightBase.Sound = MenuItemSelected.CreateInstance();
                    RightBase.NextMenu = "Strategic";
                    RightBase.Font = Font;
                    RightBase.TextPosition = -RightBase.Origin + new Vector2(60, 23);
                    RightBase.Text = LocalizationData.Select;
                    Three.Controls.Add(LeftBase);
                    Three.Controls.Add(RightBase);

                    EpilogueMenu eMenu = new EpilogueMenu();
                    Manager.Menues.Add("Epilogue", eMenu);


                }

            }
            else if (e.Menu == Manager.Menues["Strategic"])
            {
                if (TutorialMode)
                {
                    if (TutorialProgress == 0)
                    {
                        e.Menu.MessageBox.Text = LocalizationData.Tutorial0;
                        e.Menu.MessageBox.Show(true);
                        e.Menu.MessageBox.OnClosed += new Action(MessageBox_OnClosed);
                        TutorialProgress++;
                    }
                }
                if (GalaxyImage == null)
                    GalaxyImage = new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Strategic/06"), CanOffset = true, Depth = 0.9f };
                
                ScienceDNA = RCS.GetObject<Texture2D>("Menues/Strategic/science_screen");
                textManager.Settings.Width = 200;
                textManager.Settings.Offset = new Vector2(545, 50);
                textManager.Text = "";
                textManager.Parse();


                e.Menu.Controls.Clear();
                e.Menu.Controls.Add(new AdControl(adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
                HybridButton LeftStrategic = new HybridButton(RCS.GetObject<Texture2D>("Menues/LeftButton"), RCS.GetObject<Texture2D>("Menues/LeftButton"), new Vector2(43, 444));
                LeftStrategic.Origin = new Vector2(0, 62);
                LeftStrategic.NextMenu = "%Back%";
                LeftStrategic.UseSound = true;
                LeftStrategic.HoverColor = new Color(50, 238, 50);
                LeftStrategic.Sound = MenuItemSelected.CreateInstance();
                LeftStrategic.CanOffset = false;
                LeftStrategic.Depth = 0.01f;
                LeftStrategic.Font = Font;
                LeftStrategic.TextPosition = -LeftStrategic.Origin + new Vector2(50, 23);
                LeftStrategic.Text = LocalizationData.Back;
                HybridButton RightStrategic = new HybridButton(RCS.GetObject<Texture2D>("Menues/RightButton"), RCS.GetObject<Texture2D>("Menues/RightButton"), new Vector2(757, 444));
                RightStrategic.Origin = new Vector2(172, 62);
                RightStrategic.NextMenu = "Mission";
                RightStrategic.CanOffset = false;
                RightStrategic.UseSound = true;
                RightStrategic.HoverColor = new Color(50, 238, 50);
                RightStrategic.Sound = MenuItemSelected.CreateInstance();
                RightStrategic.Depth = 0.01f;
                RightStrategic.Font = Font;
                RightStrategic.TextPosition = -RightStrategic.Origin + new Vector2(60, 23);
                RightStrategic.Text = LocalizationData.Select;


                e.Menu.Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Strategic/Strategy"), Position = Vector2.Zero, Depth = 0.1f });
                e.Menu.Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Strategic/Screen"), Position = Vector2.Zero, Depth = 0.7f });
                e.Menu.Controls.Add(GalaxyImage);
                e.Menu.Controls.Add(LeftStrategic);
                e.Menu.Controls.Add(RightStrategic);

                
                int TextureNumber = playerData.Research["Growth"] + 1;
                if (TextureNumber >= 6)
                {
                    TextureNumber = 6;
                }
                TexturedButton Science1 = new TexturedButton(RCS.GetObject<Texture2D>(string.Format("Science/1_{0}", TextureNumber)), RCS.GetObject<Texture2D>(string.Format("Science/1_{0}", TextureNumber)), new Vector2(100, 10) - new Vector2(37, 319));
                TextureNumber = playerData.Research["Speed"] + 1;
                if (TextureNumber >= 6)
                {
                    TextureNumber = 6;
                }
                TexturedButton Science2 = new TexturedButton(RCS.GetObject<Texture2D>(string.Format("Science/2_{0}", TextureNumber)), RCS.GetObject<Texture2D>(string.Format("Science/2_{0}", TextureNumber)), new Vector2(100 + 164, 10) - new Vector2(37, 319));
                TextureNumber = playerData.Research["Attack"] + 1;
                if (TextureNumber >= 6)
                {
                    TextureNumber = 6;
                }
                TexturedButton Science3 = new TexturedButton(RCS.GetObject<Texture2D>(string.Format("Science/3_{0}", TextureNumber)), RCS.GetObject<Texture2D>(string.Format("Science/3_{0}", TextureNumber)), new Vector2(100, 10 + 164) - new Vector2(37, 319));
                TextureNumber = playerData.Research["Defense"] + 1;
                if (TextureNumber >= 6)
                {
                    TextureNumber = 6;
                }
                TexturedButton Science4 = new TexturedButton(RCS.GetObject<Texture2D>(string.Format("Science/4_{0}", TextureNumber)), RCS.GetObject<Texture2D>(string.Format("Science/1_{0}", TextureNumber)), new Vector2(100 + 164, 10 + 164) - new Vector2(37, 319));


                Science1.Visible = Science1.Enabled = true;
                Science1.Scale = 0.640625f;
                Science2.Visible = Science2.Enabled = true;
                Science2.Scale = 0.640625f;
                Science3.Visible = Science3.Enabled = true;
                Science3.Scale = 0.640625f;
                Science4.Visible = Science4.Enabled = true;
                Science4.Scale = 0.640625f;
                Science1.Tag = "Science1";
                Science2.Tag = "Science2";
                Science3.Tag = "Science3";
                Science4.Tag = "Science4";
                Science1.OnClick += ScienceOnClick;
                Science2.OnClick += ScienceOnClick;
                Science3.OnClick += ScienceOnClick;
                Science4.OnClick += ScienceOnClick;
                Science1.UseSound = true;
                Science2.UseSound = true;
                Science3.UseSound = true;
                Science4.UseSound = true;
                Science1.Sound = MenuItemSelected.CreateInstance();
                Science2.Sound = MenuItemSelected.CreateInstance();
                Science3.Sound = MenuItemSelected.CreateInstance();
                Science4.Sound = MenuItemSelected.CreateInstance();
                e.Menu.Controls.Add(Science1);
                e.Menu.Controls.Add(Science2);
                e.Menu.Controls.Add(Science3);
                e.Menu.Controls.Add(Science4);
                int tC = 4;
                Control[] controls = (from c in e.Menu.Controls where CheckScienceTag(c.Tag) select c).ToArray();
                foreach (Control control in controls)
                {
                    Image image = new Image();
                    image.CanOffset = false;
                    image.Position = (control as TexturedButton).Position;
                    image.Enabled = true;
                    image.Visible = true;
                    image.Scale = 1f;
                    image.Depth = 0.72f;
                    image.Color = Color.White;
                    image.Tag = "Science" + (tC.ToString());
                    tC++;
                    image.Texture = RCS.GetObject<Texture2D>("Science/science_icon_bg");
                    e.Menu.Controls.Add(image);
                }
                EpisodeProgressCounter = 0;
                HexControl hexControl = new HexControl(RCS.GetObject<Texture2D>("Menues/Strategic/Hex2"), factory.GetHexFill(410, new Vector2(512, 512)));
                hexControl.Depth = 0.8f;
                foreach (Episode episode in selectedCampaing.Episodes)
                {
                    if (EpisodeProgressCounter < playerData.GetProgress(selectedCampaing.InternalName))
                    {
                        selectedEpisode = episode;
                        selectedMission = selectedEpisode.Missions[0];
                        hexControl[episode.Position].Color = Color.Yellow * 0.7f;
                        EpisodeProgressCounter += episode.Missions.Count;
                    }
                    else
                    {
                        hexControl[episode.Position].Color = Color.White * 0.8f;
                    }

                    if (EpisodeProgressCounter < playerData.GetProgress(selectedCampaing.InternalName))
                    {
                        hexControl[episode.Position].Color = new Color(53, 234, 28) * 0.7f;
                        foreach (int cell in episode.Cells)
                            hexControl[cell].Color = new Color(53, 234, 28) * 0.7f;
                    }
                    hexControl[episode.Position].Tag = episode;
                    hexControl[episode.Position].OnClick += new Action<Cell>(hexControl_OnClick);
                }
                textManager.Text = selectedEpisode.Name + "\n\n\n\n\n" + selectedEpisode.Description;
                textManager.Parse();
                e.Menu.Controls.Add(hexControl);
                /*
                foreach (Episode episode in selectedCampaing.Episodes)
                {
                    AnimatedButton animated = new AnimatedButton(SSL.DescriptorList["StrategicEpisodeSelection"]);
                    animated.Position = episode.Position;
                    animated.Origin = new Vector2(32, 32);
                    if (EpisodeProgressCounter < playerData.GetProgress(selectedCampaing.InternalName))
                    {
                        animated.Enabled = true;
                        EpisodeProgressCounter += episode.Missions.Count;
                    }
                    else
                    {
                        animated.Enabled = false;
                    }

                    animated.CanOffset = true;
                    animated.Depth = 0.8f;
                    animated.Color = animated.Enabled ? Color.Khaki : Color.Gray;
                    animated.Tag = episode;
                    animated.OnClick += new EventHandler(animated_OnClick);
                    e.Menu.Controls.Add(animated);


                
                }
                */
                EpisodeProgressCounter = 0;
                foreach (Episode ep in selectedCampaing.Episodes)
                {
                    if (ep != selectedEpisode)
                        EpisodeProgressCounter += ep.Missions.Count;
                    else
                        break;
                }

                
                e.Menu.InitializeControls();
            }
            else if (e.Menu == Manager.Menues["Mission"])
            {
                if (TutorialMode)
                    if (TutorialProgress == 4)
                    {
                        e.Menu.MessageBox.Text = LocalizationData.Tutorial4;
                        e.Menu.MessageBox.Show(true);
                        TutorialProgress++;
                    }
                MissionOffset = Vector2.Zero;
                textManager.Settings.Width = 200;
                textManager.Settings.Offset = new Vector2(545, 50);
                
                e.Menu.Controls.Clear();
                e.Menu.Controls.Add(new AdControl(adManager) { Position = new Vector2(252, 415), Scale = 1, Depth = 0 });
                HybridButton LeftMission = new HybridButton(RCS.GetObject<Texture2D>("Menues/LeftButton"), RCS.GetObject<Texture2D>("Menues/LeftButton"), new Vector2(43, 444));
                LeftMission.Origin = new Vector2(0, 62);
                LeftMission.NextMenu = "%Back%";
                LeftMission.CanOffset = false;
                LeftMission.Depth = 0.01f;
                LeftMission.HoverColor = new Color(50, 238, 50);
                LeftMission.UseSound = true;
                LeftMission.Sound = MenuItemSelected.CreateInstance();
                LeftMission.Font = Font;
                LeftMission.TextPosition = -LeftMission.Origin + new Vector2(50, 23);
                LeftMission.Text = LocalizationData.Back;
                HybridButton RightMission = new HybridButton(RCS.GetObject<Texture2D>("Menues/RightButton"), RCS.GetObject<Texture2D>("Menues/RightButton"), new Vector2(757, 444));
                RightMission.Origin = new Vector2(172, 62);
                RightMission.CanOffset = false;
                RightMission.Depth = 0.01f;
                RightMission.HoverColor = new Color(50, 238, 50);
                RightMission.Font = Font;
                RightMission.UseSound = true;
                RightMission.Sound = MenuItemSelected.CreateInstance();
                RightMission.TextPosition = -RightMission.Origin + new Vector2(60, 23);
                RightMission.Text = LocalizationData.Play;
                RightMission.OnClick += new EventHandler(RightMission_OnClick);

                e.Menu.Controls.Add(LeftMission);
                e.Menu.Controls.Add(RightMission);
                e.Menu.Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Strategic/Strategy"), Position = Vector2.Zero, Depth = 0.1f });
                e.Menu.Controls.Add(new Image() { Texture = RCS.GetObject<Texture2D>("Menues/Strategic/Screen"), Position = Vector2.Zero, Depth = 0.7f });
                e.Menu.Controls.Add(PopupMB);

                int Counter = EpisodeProgressCounter;
                if (EpisodeProgressCounter == playerData.GetProgress(selectedCampaing.InternalName) - 1)
                {
                    if (!String.IsNullOrWhiteSpace(selectedEpisode.EpisodePopup))
                    {
                        PopupTextManager.Text = selectedEpisode.EpisodePopup;
                        PopupTextManager.Parse();
                        PopupMB.Show(true);
                    }
                }
                foreach (Mission mission in selectedEpisode.Missions)
                {
                    Vector2 Position = Vector2.Zero;
                    if (Counter - EpisodeProgressCounter == 0 || Counter - EpisodeProgressCounter == selectedEpisode.Missions.Count - 1)
                    {
                        Position = new Vector2(150 + (Counter - EpisodeProgressCounter) * 256, 240);
                    }
                    else
                    {
                        bool Even = (Counter - EpisodeProgressCounter - 1) % 2 == 0;
                        if (Even)
                        {
                            Position = new Vector2(150 + (Counter - EpisodeProgressCounter) * 256, 140);
                        }
                        else
                        {
                            Position = new Vector2(150 + (Counter - EpisodeProgressCounter) * 256, 340);
                        }
                    }
                    Position.Y -= 30;
                    TexturedButton tb = new TexturedButton(RCS.GetObject<Texture2D>("Menues/Strategic/selection_tactical"), RCS.GetObject<Texture2D>("Menues/Strategic/selection_tactical"), Position);
                    tb.CanOffset = true;
                    tb.Scale = 0.5f;
                    tb.Origin = new Vector2(128, 128);
                    tb.Color = Counter < playerData.GetProgress(selectedCampaing.InternalName) ? Color.White : Color.Black;
                    if (tb.Color == Color.White)
                        selectedMission=mission;
                    tb.Depth = 0.8f;
                    tb.Enabled = Counter < playerData.GetProgress(selectedCampaing.InternalName) ? true : false;
                    tb.Tag = mission;
                    tb.OnClick += new EventHandler(tb_OnClick);
                    e.Menu.Controls.Add(tb);
                    Counter++;

                }
                (from control in e.Menu.Controls where control.Tag == selectedMission select control).First().Color = Color.Gray;
                textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
                textManager.Parse();
                e.Menu.InitializeControls();
            }
        }



        void MessageBox_OnClosed()
        {
            if (TutorialMode)
            {
                if (TutorialProgress == 1)
                {
                    Manager.Menues["Strategic"].MessageBox.Text = LocalizationData.Tutorial1;
                    Manager.Menues["Strategic"].MessageBox.Show(true);
                    TutorialProgress++;
                }
                else if (TutorialProgress == 2)
                {
                    Manager.Menues["Strategic"].MessageBox.Text = LocalizationData.Tutorial2;
                    Manager.Menues["Strategic"].MessageBox.Show(true);
                    TutorialProgress++;
                }
            }
        }
        void Manager_OnEndMenuChange(object sender, MenuChangeEventArgs e)
        {
            if (Manager.SelectedMenu == "Base")
            {
                textSettings.Width = 670;
                textSettings.Offset = new Vector2(67, 54);
                textManager.Text = selectedCampaing.Name + "\n\n" + selectedCampaing.LongDescription;
                textManager.Parse();
            }
            else if (Manager.SelectedMenu == "Strategic")
            {
                GalaxyImage.Offset = new Vector2(-200, -300);
                foreach (Control control in Manager.Menues["Strategic"].Controls)
                {
                    HexControl ab = control as HexControl;
                    if (ab != null)
                    {
                        ab.Offset = GalaxyImage.Offset;
                    }
                }
            }
        }

        void SetupCameraSmall()
        {
            camera = new Camera();
            camera.Position = new Vector2(400, 240);
            camera.Zoom = 1f;
        }
        void SetupCameraBig()
        {
            camera = new Camera();
            camera.Position = new Vector2(800, 480);
            camera.Zoom = 0.5f;
        }

        bool CheckScienceTag(object Tag)
        {
            if (Tag == null)
                return false;
            else
            {
                string stringTag = Tag.ToString();
                return stringTag.StartsWith("Science");
            }
        }

        void OnGameEnd()
        {
            if (playerData.ReportingEnabled)
            {
                VelesConflictReporting.GameUsageStastics gus = new VelesConflictReporting.GameUsageStastics();
                gus.Actions = GameActions;
                gus.Map = selectedMission.Map;
                gus.TimeSpent = GameTimeSpent;
                gus.Difficulty = (int)playerData.Difficulty;
                gus.DeviceID = DeviceID;
                gus.Winner = GameManager.GetLooser() == PlayerType.Player2 ? 1 : 2;
                lock (GameUsage)
                    GameUsage.Add(gus);
            }
            GameActions = 0;
            GameTimeSpent = TimeSpan.Zero;
            Manager.Enabled = true;
            MainMenuContract.Enabled = true;
            SinglePlayerContract.Enabled = false;
            GameScript.Dispose();
            GameScript = null;
            GameState = GameState.Menu;
            LevelContentManager.Unload();
            LevelContentManager.Dispose();

            textSettings.Scale = 1;
            textSettings.Origin = Vector2.Zero;
            textSettings.Width = 200;
            textSettings.Offset = new Vector2(545, 50);
            textSettings.Depth = 0.49f;
            //Select the next mission
            if (GameManager.GetLooser() == PlayerType.Player2)
            {
                Control LastControl = Manager.Menues["Mission"].Controls.Last(control => control.Tag == selectedMission);
                LastControl.Color = Color.White;
                int Index = Manager.Menues["Mission"].Controls.LastIndexOf(LastControl) + 1;
                int Offset = Manager.Menues["Mission"].Controls.IndexOf(Manager.Menues["Mission"].Controls.First(control => control.Tag is Mission));
                int Progress = EpisodeProgressCounter + selectedEpisode.Missions.IndexOf(selectedMission) + 2;
                if (Progress > playerData.GetProgress(selectedCampaing.InternalName))
                {
                    playerData.Points += selectedMission.PointsGain;
                    playerData.SetProgress(selectedCampaing.InternalName, Progress);
                }
                Mission oldMission = selectedMission;
                if (Index >= Manager.Menues["Mission"].Controls.Count)
                {
                    //Switch episodes
                    HexControl hexControl = (HexControl)Manager.Menues["Strategic"].Controls.First(c => c is HexControl);
                    foreach (int cell in selectedEpisode.Cells)
                    {
                        hexControl[cell].Color = new Color(53, 234, 28) * 0.7f;
                    }
                    hexControl[selectedEpisode.Position].Color = new Color(53, 234, 28) * 0.7f;
                    Index = selectedCampaing.Episodes.IndexOf(selectedEpisode) + 1;

                    if (Index >= selectedCampaing.Episodes.Count)
                    {
                        textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
                        textManager.Parse();

                        //Last episode
                        Manager.ClearHistory();
                        Manager.InjectMenu(Manager.Menues["Main"]);
                        if (!string.IsNullOrWhiteSpace(oldMission.Popup))
                        {
                            PopupTextManager.Text = oldMission.Popup;
                            PopupTextManager.Parse();
                            ShowEpilogueAfter = true;
                            PopupMB.Show(true);

                        }
                        else
                        {
                            (Manager.Menues["Epilogue"] as EpilogueMenu).Prepare(selectedCampaing);
                            Manager.SetMenu("Epilogue");
                        }
                    }
                    else
                    {

                        selectedEpisode = selectedCampaing.Episodes[Index];
                        hexControl[selectedEpisode.Position].Color = Color.Yellow * 0.5f;
                        EpisodeProgressCounter = 0;
                        foreach (Episode ep in selectedCampaing.Episodes)
                        {
                            if (ep != selectedEpisode)
                                EpisodeProgressCounter += ep.Missions.Count;
                            else
                                break;
                        }
                        //Trigget menu change event to rebuild the mission
                        //Manager_OnBeginMenuChange(this, new MenuChangeEventArgs() { Menu = Manager.Menues["Mission"] });
                        if (!string.IsNullOrWhiteSpace(oldMission.Popup))
                        {
                            ShowStrategicAfter = true;
                            textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
                            textManager.Parse();
                            PopupTextManager.Text = oldMission.Popup;
                            PopupTextManager.Parse();
                            PopupMB.Show(true);
                        }
                        else
                        {
                            Manager.SetMenu("Strategic");
                            Manager.ClearHistory();
                            Manager.InjectMenu(Manager.Menues["Main"]);
                            Manager.InjectMenu(Manager.Menues["Campaing"]);
                            Manager.InjectMenu(Manager.Menues["Base"]);
                        }
                    }
                }
                else
                {
                    LastControl = Manager.Menues["Mission"].Controls[Index];
                    LastControl.Color = Color.Gray;
                    LastControl.Enabled = true;
                    if (!string.IsNullOrWhiteSpace(selectedMission.Popup))
                    {
                        PopupTextManager.Text = selectedMission.Popup;
                        PopupTextManager.Parse();
                        PopupMB.Show(true);
                    }
                    selectedMission = LastControl.Tag as Mission;



                    textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
                    textManager.Parse();
                }
            }
            else
            {
                textManager.Text = selectedMission.Name + "\n\n\n\n\n" + selectedMission.Description;
                textManager.Parse();
                PopupTextManager.Text = LocalizationData.GG;
                PopupTextManager.Parse();
                PopupMB.Show(true);
            }
            playerData.Save();
            GameManager = null;
        }

        void fOnUpdateStart(GameTime gameTime)
        {

            //Push if over ten
            if (GameUsage.Count > 5 && !UpdatingGameUsage)
            {
                TryPushStatistics();
            }

            if (TouchPanel.GetState().Count == 0)
            {
                CanDrag = true;
                Draging = false;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && GameState == GameState.Menu)
            {
                if (PopupMB.Visible == true)
                {
                    PopupMB.Close();
                }
                else if (Manager.Menues[Manager.SelectedMenu].MessageBox != null && Manager.Menues[Manager.SelectedMenu].MessageBox.Visible && !Manager.Menues[Manager.SelectedMenu].MessageBox.Locked)
                {
                    
                    Manager.Menues[Manager.SelectedMenu].MessageBox.Close();
                }
                else
                {
                    Manager.PreviousMenu();
                }
            }
            else if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && GameState == GameState.Singleplayer)
            {
                if (MessageBox != null && MessageBox.Visible)
                {
                    MessageBox.Close();
                    InternalPause = false;
                    GameScript.Pause = false;
                }
                else
                {
                    GameScript.Pause = true;
                    InternalPause = true;
                    GameState = GameState.Pause;
                    PauseManager.Enabled = true;
                    PauseManager.Retransition();
                }
            }
            else if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && GameState == GameState.Pause)
            {
                GameScript.Pause = false;
                InternalPause = false;
                GameState = GameState.Singleplayer;
                PauseManager.Enabled = false;
            }

            if (GameState == GameState.Singleplayer)
            {
                if (MediaPlayer.GameHasControl)
                {
                    MediaPlayer.Volume = 0.6f;
                }
            }
            else if (GameState == GameState.Menu)
            {
                if (MediaPlayer.GameHasControl)
                {
                    MediaPlayer.Volume = 0.6f;
                }
            }
            
        }
        void fOnUpdateEnd(GameTime gameTime)
        {
            #region Menu
            if (GameState == GameState.Menu)
            {
                if (Manager.SelectedMenu == "Mission" && Manager.State == MenuState.Sustain)
                {
                    offsetVelocity *= 0.95f;
                    MissionOffset = Vector2.Clamp(MissionOffset + offsetVelocity, new Vector2(-(Manager.Menues["Mission"].Controls.Count - 3) * 240 + 400, 0), new Vector2(0, 0));
                    foreach (Control control in Manager.Menues["Mission"].Controls)
                    {
                        TexturedButton ab = control as TexturedButton;
                        if (ab != null)
                        {
                            ab.Offset = MissionOffset;
                        }
                    }
                }
                else if (Manager.SelectedMenu == "Strategic" && Manager.State == MenuState.Sustain)
                {
                    if (!ScienceVisible)
                    {
                        if (!textManager.Text.StartsWith(selectedEpisode.Name))
                        {
                            textManager.Text = selectedEpisode.Name + "\n\n\n\n\n" + selectedEpisode.Description;
                            textManager.Parse();
                        }
                        offsetVelocity *= 0.95f;
                        float Multiplier = (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000f / 30);
                        GalaxyImage.Offset = Vector2.Clamp(GalaxyImage.Offset + offsetVelocity * Multiplier, new Vector2(-224 - 310, -480), new Vector2(0, 0));
                        foreach (Control control in Manager.Menues["Strategic"].Controls)
                        {
                            HexControl ab = control as HexControl;
                            if (ab != null)
                            {
                                ab.Offset = GalaxyImage.Offset;
                            }
                        }
                    }
                    else
                        offsetVelocity = Vector2.Zero;
                    if (!Draging && ScienceAnimator.State == AnimatorState.Stoped && ScienceAnimatorReversed.State == AnimatorState.Stoped)
                    {
                        if (ScienceState == ScienceState.Forward)
                        {
                            if (Vector2.Distance(SciencePosition, new Vector2(462, 8)) > 100)
                            {
                                ScienceAnimator.AtStart = SciencePosition;
                                ScienceAnimator.Reset();
                                ScienceAnimator.Start();
                                ScienceLock = true;

                                if (TutorialMode)
                                {
                                    if (TutorialProgress == 3)
                                    {
                                        Manager.Menues["Strategic"].MessageBox.Text = LocalizationData.Tutorial3;
                                        Manager.Menues["Strategic"].MessageBox.Show(true);
                                        TutorialProgress++;
                                    }
                                }
                            }
                        }
                        else if (ScienceState == ScienceState.Reversed)
                        {
                            if (Vector2.Distance(SciencePosition, new Vector2(462, 8)) < 460)
                            {
                                ScienceAnimatorReversed.AtStart = SciencePosition;
                                ScienceAnimatorReversed.Reset();
                                ScienceAnimatorReversed.Start();
                            }
                        }
                    }
                    ScienceAnimator.Update(gameTime);
                    ScienceAnimatorReversed.Update(gameTime);
                    if (!Draging)
                    {
                        if (ScienceState == ScienceState.Forward)
                        {
                            SciencePosition = ScienceAnimator.Value;
                            if (ScienceAnimator.State == AnimatorState.Started && Vector2.Distance(SciencePosition, new Vector2(462, 8)) > 520)
                            {
                                ScienceAnimator.AtStart = new Vector2(462, 8);
                                ScienceState = ScienceState.Reversed;
                                ScienceAnimatorReversed.Reset();
                            }
                        }
                        else
                        {
                            SciencePosition = ScienceAnimatorReversed.Value;
                            if (ScienceAnimatorReversed.State == AnimatorState.Started && Vector2.Distance(SciencePosition, new Vector2(462, 8)) < 10)
                            {
                                ScienceAnimatorReversed.AtStart = new Vector2(25, 319);
                                ScienceLock = false;
                                ScienceState = ScienceState.Forward;
                                ScienceAnimator.Reset();
                            }
                            else if (ScienceAnimatorReversed.State == AnimatorState.Started)
                            {
                                ScienceVisible = false;
                            }
                            else if (ScienceAnimatorReversed.State == AnimatorState.Stoped)
                            {
                                ScienceVisible = true;
                            }
                        }
                    }
                    foreach (Control control in Manager.Menues["Strategic"].Controls.Where(c => CheckScienceTag(c.Tag)))
                    {
                        if (!(control is Image))
                            control.Depth = 0.71f;
                        control.CanOffset = true;
                        control.Offset = SciencePosition;
                    }
                    if (ScienceVisible)
                    {
                        foreach (Control control in Manager.Menues["Strategic"].Controls)
                        {
                            if (control is HybridButton)
                            {
                                HybridButton hb = control as HybridButton;
                                if (hb.Text == LocalizationData.Select)
                                {
                                    selectedResearch = null;
                                    hb.NextMenu = "";
                                    hb.OnClick += new EventHandler(hb_OnClick);
                                    hb.Text = LocalizationData.Research;
                                    break;
                                }
                            }
                        }
                        foreach (Control control in Manager.Menues["Strategic"].Controls)
                        {
                            if (control is HybridButton)
                            {
                                HybridButton hb = control as HybridButton;
                                if (hb.Text == LocalizationData.Back)
                                {
                                    hb.NextMenu = "";
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Control control in Manager.Menues["Strategic"].Controls)
                        {
                            if (control is HybridButton)
                            {
                                HybridButton hb = control as HybridButton;
                                if (hb.Text == LocalizationData.Research)
                                {
                                    hb.OnClick -= hb_OnClick;
                                    hb.NextMenu = "Mission";
                                    hb.Text = LocalizationData.Select;
                                    break;
                                }
                            }
                        }
                        foreach (Control control in Manager.Menues["Strategic"].Controls)
                        {
                            if (control is HybridButton)
                            {
                                HybridButton hb = control as HybridButton;
                                if (hb.Text == LocalizationData.Back)
                                {
                                    hb.NextMenu = "%Back%";
                                    break;
                                }
                            }
                        }
                    }
                    if (SciencePosition.X < 27)
                        SciencePosition.X = 27;
                    else if (SciencePosition.X > 462)
                        SciencePosition.X = 462;
                    if (SciencePosition.Y < 8)
                        SciencePosition.Y = 8;
                    else if (SciencePosition.Y > 319)
                        SciencePosition.Y = 319;

                    ScienceRectangle.Width = 512 - (int)SciencePosition.X;
                    ScienceRectangle.Height = (int)SciencePosition.Y - 8 + 103;
                    ScienceRectangle.Y = 305 - ((int)SciencePosition.Y - 8);
                }
            }
            #endregion
            #region SinglePlayer
            else if (GameState == GameState.Singleplayer)
            {
                cameraVelocity *= 0.95f;
                camera.Position += cameraVelocity;
                Vector2 cameraWorldMin = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.Transforms));
                Vector2 cameraSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / camera.Zoom;
                Vector2 limitWorldMin = new Vector2(0, 0);
                Vector2 limitWorldMax = new Vector2(Width, Height);
                Vector2 positionOffset = camera.Position - cameraWorldMin;
                camera.Position = Vector2.Clamp(cameraWorldMin, limitWorldMin, limitWorldMax - cameraSize) + positionOffset;
                if (!InternalPause)
                {
                    GameTimeSpent += gameTime.ElapsedGameTime;
                    foreach (Planet p in GameManager.State.Planets)
                    {
                        if (GameSelectedPlanets.Contains(p))
                            p.SelectionRotation += 0.05f;
                        else
                            p.SelectionRotation = 0;
                    }
                    GameManager.Update();
                    AI.ProcessTurn(GameManager, gameTime);
                    if (GameManager.GameEnd())
                    {
                        OnGameEnd();
                    }
                }


            }
            #endregion
            else if(GameState==GameState.Multiplayer)
            {

                Multiplayer.Update(gameTime);
            }
        }
        public void DisableMenues()
        {
            Manager.Enabled = false;
            MainMenuContract.Enabled = false;
        }
        public void EnableMenues()
        {
            Manager.Enabled = true;
            MainMenuContract.Enabled = true;
        }
        void DrawPauseStart()
        {
            camera.Paralax = new Vector2(0.4f);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            spriteBatch.Draw(ParalaxLayer1, new Vector2(-600, -600), Color.White);
            spriteBatch.End();

            camera.Paralax = new Vector2(0.6f);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            spriteBatch.Draw(ParalaxLayer2, new Vector2(-600, -600), Color.White);
            spriteBatch.End();

            camera.Paralax = new Vector2(0.9f);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            spriteBatch.Draw(ParalaxLayer3, new Vector2(-600, -600), Color.White);
            spriteBatch.End();
            camera.Paralax = Vector2.One;
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            foreach (Planet planet in GameManager.State.Planets)
            {
                Color DrawColor = Color.White;
                switch (planet.Owner)
                {
                    case PlayerType.Neutral:
                        DrawColor = Color.White;
                        break;
                    case PlayerType.Player1:
                        DrawColor = Color.LightGreen;
                        if (GameSelectedPlanets.Contains(planet))
                        {
                            DrawColor = Color.GreenYellow;
                        }
                        break;
                    case PlayerType.Player2:
                        DrawColor = Color.DarkRed;
                        break;
                }
                spriteBatch.Draw(planet.Texture, planet.Position, null, DrawColor, 0f, PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 0.8f);
                spriteBatch.DrawString(Font, planet.Forces.ToString(), planet.Position /*- new Vector2(32, 32) * planet.PlanetSize */, DrawColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.79f);
            }
            foreach (Fleet fleet in GameManager.State.Fleets)
            {
                if (fleet.Dead)
                    continue;
                Texture2D FleetTexture = fleet.Owner == PlayerType.Player1 ? FleetTexture1 : FleetTexture2;
                spriteBatch.Draw(FleetTexture, fleet.Position, null, Color.White, fleet.Rotation, FleetOrigin, 0.3f, SpriteEffects.None, 0.7f);
                foreach (Vector2 p in fleet.Positions)
                {
                    spriteBatch.Draw(FleetTexture, p + fleet.Position, null, Color.White, fleet.Rotation, FleetOrigin, 0.3f, SpriteEffects.None, 0.7f);
                }
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
        }
        void fOnDrawStart(GameTime gameTime)
        {


            if (GameState == GameState.Singleplayer)
            {
                GraphicsDevice.SamplerStates[0] = new SamplerState() { MaxAnisotropy = 16, Filter = TextureFilter.Anisotropic };
                camera.Paralax = new Vector2(0.4f);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
                spriteBatch.Draw(ParalaxLayer1, new Vector2(-600, -600), Color.White);
                spriteBatch.End();

                camera.Paralax = new Vector2(0.6f);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
                spriteBatch.Draw(ParalaxLayer2, new Vector2(-600, -600), Color.White);
                spriteBatch.End();

                camera.Paralax = new Vector2(0.9f);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
                spriteBatch.Draw(ParalaxLayer3, new Vector2(-600, -600), Color.White);
                spriteBatch.End();
                camera.Paralax = Vector2.One;
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            }
            else if (GameState == GameState.Editor)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.Transforms);
            }
            else if (GameState == GameState.Pause)
            {
                DrawPauseStart();
            }
            else if(GameState==GameState.Multiplayer)
            {
                Multiplayer.Draw();
            }
            else if (Manager.SelectedMenu != "Exit")
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                spriteBatch.Draw(Background.Instance, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            }
        }
        void fOnDrawEnd(GameTime gameTime)
        {
            if (PopupMB.Visible)
            {
                PopupTextSettings.Scale = PopupMB.Scale;
                PopupTextManager.Draw(spriteBatch);
            }
            if (GameState == GameState.Singleplayer)
            {
                if (MessageBox != null && MessageBox.Visible)
                {
                    MessageBox.Draw(spriteBatch);
                }
                Rectangle cameraRectangle = new Rectangle(0, 0, (int)(GraphicsDevice.Viewport.Width / camera.Zoom), (int)(GraphicsDevice.Viewport.Height / camera.Zoom));
                Rectangle cullRectangle = new Rectangle(0, 0, 10, 10);
                cameraRectangle.Offset((int)(camera.Position.X - 400 / camera.Zoom), (int)(camera.Position.Y - 240 / camera.Zoom));
                Texture2D pSelection = RCS.GetObject<Texture2D>("selection_planet_ingame").Instance;
                foreach (Planet planet in GameManager.State.Planets)
                {
                    cullRectangle.X = cullRectangle.Y = 0;
                    cullRectangle.Offset((int)(planet.Position.X - PlanetOrigin.X * planet.PlanetSize), (int)(planet.Position.Y - PlanetOrigin.Y * planet.PlanetSize));
                    cullRectangle.Width = (int)(planet.Texture.Width * planet.PlanetSize);
                    cullRectangle.Height = (int)(planet.Texture.Height * planet.PlanetSize);
                    if(!cameraRectangle.Intersects(cullRectangle))
                    {
                        continue;
                    }
                    Color DrawColor = Color.White;
                    switch (planet.Owner)
                    {
                        case PlayerType.Neutral:
                            DrawColor = Color.White;
                            break;
                        case PlayerType.Player1:
                            DrawColor = Color.LightGreen;
                            if (GameSelectedPlanets.Contains(planet))
                            {
                                DrawColor = Color.GreenYellow;
                            }
                            break;
                        case PlayerType.Player2:
                            DrawColor = Color.DarkRed;
                            break;
                    }
                    spriteBatch.Draw(planet.Texture, planet.Position, null, DrawColor, 0f, PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 0.2f);
                    if (DrawColor == Color.GreenYellow)
                    {
                        spriteBatch.Draw(pSelection, planet.Position, null, DrawColor, planet.SelectionRotation, PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 0.2f);
                    }
                    spriteBatch.DrawString(Font, planet.Forces.ToString(), planet.Position, DrawColor,0f,Vector2.Zero,1f,SpriteEffects.None,0.19f);
                }
                foreach (Fleet fleet in GameManager.State.Fleets)
                {
                    Texture2D FleetTexture = fleet.Owner == PlayerType.Player1 ? FleetTexture1 : FleetTexture2;
                    if (fleet.Dead)
                        continue;
                    cullRectangle.X = cullRectangle.Y = 0;
                    cullRectangle.Offset((int)(fleet.Position.X - FleetOrigin.X * 0.3f), (int)(fleet.Position.Y - FleetOrigin.Y * 0.3f));
                    cullRectangle.Width = (int)(FleetTexture.Width * 0.5f);
                    cullRectangle.Height = (int)(FleetTexture.Height * 0.5f);
                    if (!cameraRectangle.Intersects(cullRectangle))
                    {
                        continue;
                    }
                    
                    spriteBatch.Draw(FleetTexture, fleet.Position, null, Color.White, fleet.Rotation, FleetOrigin, 0.3f, SpriteEffects.None, 0f);
                    foreach (Vector2 p in fleet.Positions)
                    {
                        spriteBatch.Draw(FleetTexture, p + fleet.Position, null, Color.White, fleet.Rotation, FleetOrigin, 0.3f, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.End();
                spriteBatch.Begin();
                spriteBatch.Draw(GameOverlay.Instance, Vector2.Zero, Color.White);
            }
            else if (GameState == GameState.Editor)
            {
                lock (GameBase.GameManager)
                {
                    foreach (Planet planet in GameManager.State.Planets)
                    {
                        if (planet.Texture == null)
                        {
                            planet.Texture = LevelContentManager.Load<Texture2D>("Graphics/Planets/Large/Populated/1");
                        }
                        Color DrawColor = Color.White;
                        switch (planet.Owner)
                        {
                            case PlayerType.Neutral:
                                DrawColor = Color.White;
                                break;
                            case PlayerType.Player1:
                                DrawColor = Color.LightGreen;
                                break;
                            case PlayerType.Player2:
                                DrawColor = Color.DarkRed;
                                break;
                        }
                        spriteBatch.Draw(planet.Texture, planet.Position, null, DrawColor, 0f, PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 0.2f);
                        spriteBatch.DrawString(Font, planet.Forces.ToString(), planet.Position, DrawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.19f);
                    }
                }
            }
            spriteBatch.End();
        }


    }
}
