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
using System.Xml;
using VelesConflictRemote;

namespace Editor
{
    public enum PlayerType
    {
        Player1=1, Player2=2, Neutral=0
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class EditorBase : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        IntPtr MonitorHandle;
        SpriteFont GameFont;
        Camera Camera;
        Texture2D PlanetTexture;
        List<Planet> Planets;
        MouseState OldState;
        Random rnd = new Random();
        int NextID = 0;
        public Planet Selected;
        public EventHandler<EventArgs> SelectionChanged;
        PrimitiveLine Line;
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        public static RemoteWorker remoteWorker;
        public static PhoneConnection defaultConnection;

        public EditorBase(IntPtr MonitorHandle)
        {
            this.MonitorHandle = MonitorHandle;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Form.FromHandle(Window.Handle).VisibleChanged += new EventHandler(EditorBase_VisibleChanged);
            Content.RootDirectory = "Content";
            
            
            
        }
        void EditorBase_VisibleChanged(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)
                System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;
        }
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = MonitorHandle;
            Mouse.WindowHandle = MonitorHandle;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera = new Camera();
            Camera.Position = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2);
            Camera.Zoom = 1f;
            Camera.Rotation = 0f;
            remoteWorker = new RemoteWorker(null);
            remoteWorker.Run();
            base.Initialize();
        }
        float Chance = 0.25f;
        public void RandomizePlanet(Planet planet)
        {
            int type = rnd.Next(3);
            planet.PlanetSize = type == 0 ? 0.5f : type == 1 ? 0.7f : 1f;
            planet.PlanetSize += MathHelper.Lerp(-0.1f,0.1f,(float)rnd.NextDouble());
            switch (type)
            {
                case 0:
                    planet.Forces = (int)(rnd.Next(0, 6) - 3 + 8);
                    planet.Growth = 1;
                    planet.GrowthCounter = 30;
                    break;
                case 1:
                    planet.Forces = (int)(rnd.Next(0, 6) - 3 + 15);
                    planet.Growth = 4;
                    planet.GrowthCounter = 40;
                    break;
                case 2:
                    planet.Forces = (int)(rnd.Next(0, 6) - 3 + 20);
                    planet.Growth = 6;
                    planet.GrowthCounter = 50;
                    break;
            }

            if (rnd.NextDouble() < Chance)
            {
                planet.HasPeople = true;
            }
        }
        public void NewPlanet()
        {
            Planet newPlanet = new Planet();
            newPlanet.Position = Vector2.Zero;
            newPlanet.PlanetSize = 1f;
            newPlanet.ID = ++NextID;
            newPlanet.Owner = PlayerType.Neutral;
            RandomizePlanet(newPlanet);
            if (defaultConnection != null && !defaultConnection.Dead)
            {
                string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", newPlanet.ID, newPlanet.Position.X, newPlanet.Position.Y, newPlanet.PlanetSize, newPlanet.Forces, (int)newPlanet.Owner, newPlanet.GrowthCounter, newPlanet.Growth);
                defaultConnection.Write(Command);
            }
            lock (Planets)
                Planets.Add(newPlanet);
        }
        public void DeletePlanet(Planet planet)
        {
            if (planet == null)
                return;
            if (defaultConnection != null && !defaultConnection.Dead)
            {
                string Command = string.Format("+DELETE {0}\n",planet.ID);
                defaultConnection.Write(Command);
            }
            lock (Planets)
            {
                Planets.Remove(planet);
                SelectionChanged(this, EventArgs.Empty);
            }
        }
        public void ReflectVerticaly()
        {
            List<Planet> NewPlanets = new List<Planet>();
            foreach (Planet planet in Planets)
            {
                Planet newPlanet = new Planet();
                newPlanet.Position = new Vector2(MapWidth - planet.Position.X, planet.Position.Y);
                newPlanet.PlanetSize = planet.PlanetSize;
                newPlanet.ID = ++NextID;
                newPlanet.Owner = planet.Owner;
                if (defaultConnection != null && !defaultConnection.Dead)
                {
                    string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", newPlanet.ID, newPlanet.Position.X, newPlanet.Position.Y, newPlanet.PlanetSize, newPlanet.Forces, (int)newPlanet.Owner, newPlanet.GrowthCounter, newPlanet.Growth);
                    defaultConnection.Write(Command);
                }
                NewPlanets.Add(newPlanet);
            }
            lock (Planets)
            {
                Planets.AddRange(NewPlanets);
            }
        }
        public void ReflectHorizontaly()
        {
            List<Planet> NewPlanets = new List<Planet>();
            foreach (Planet planet in Planets)
            {
                Planet newPlanet = new Planet();
                newPlanet.Position = new Vector2(planet.Position.X, MapHeight-planet.Position.Y);
                newPlanet.PlanetSize = planet.PlanetSize;
                newPlanet.ID = ++NextID;
                newPlanet.Owner = planet.Owner;
                if (defaultConnection != null && !defaultConnection.Dead)
                {
                    string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", newPlanet.ID, newPlanet.Position.X, newPlanet.Position.Y, newPlanet.PlanetSize, newPlanet.Forces, (int)newPlanet.Owner, newPlanet.GrowthCounter, newPlanet.Growth);
                    defaultConnection.Write(Command);
                }
                NewPlanets.Add(newPlanet);
            }
            lock (Planets)
            {
                Planets.AddRange(NewPlanets);
            }
        }
        public void ReflectDiagonaly()
        {
            List<Planet> NewPlanets = new List<Planet>();
            foreach (Planet planet in Planets)
            {
                Planet newPlanet = new Planet();
                newPlanet.Position = new Vector2(MapWidth-planet.Position.X, MapHeight - planet.Position.Y);
                newPlanet.PlanetSize = planet.PlanetSize;
                newPlanet.ID = ++NextID;
                newPlanet.Owner = planet.Owner;
                if (defaultConnection != null && !defaultConnection.Dead)
                {
                    string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", newPlanet.ID, newPlanet.Position.X, newPlanet.Position.Y, newPlanet.PlanetSize, newPlanet.Forces, (int)newPlanet.Owner, newPlanet.GrowthCounter, newPlanet.Growth);
                    defaultConnection.Write(Command);
                }
                NewPlanets.Add(newPlanet);
            }
            lock (Planets)
            {
                Planets.AddRange(NewPlanets);
            }
        }
        public void RefreshLine()
        {
            Line = new PrimitiveLine(GraphicsDevice);
            Line.Colour = Color.Red;
            Line.AddVector(Vector2.Zero);
            Line.AddVector(new Vector2(MapWidth, 0));
            Line.AddVector(new Vector2(MapWidth, MapHeight));
            Line.AddVector(new Vector2(0, MapHeight));
            Line.AddVector(Vector2.Zero);
        }
        public void Save()
        {
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "XML file (*.xml)|*.xml";
            System.Windows.Forms.DialogResult result = save.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(save.FileName, settings);
                writer.WriteStartElement("Map");
                writer.WriteElementString("Width", MapWidth.ToString());
                writer.WriteElementString("Height", MapHeight.ToString());
                writer.WriteElementString("Format", "1");
                writer.WriteElementString("HasScript", "false");
                foreach (Planet p in Planets)
                {
                    writer.WriteStartElement("Planet");

                    writer.WriteElementString("ID", p.ID.ToString());
                    writer.WriteElementString("Owner", p.Owner.ToString());
                    writer.WriteElementString("Forces", p.Forces.ToString());
                    writer.WriteElementString("Growth", p.Growth.ToString());
                    writer.WriteElementString("GrowthCooldown", p.GrowthCounter.ToString());
                    writer.WriteElementString("Size", p.PlanetSize.ToString());
                    writer.WriteElementString("HasPeople", p.HasPeople?"true":"false");

                    writer.WriteStartElement("Position");
                    writer.WriteElementString("X", p.Position.X.ToString());
                    writer.WriteElementString("Y", p.Position.Y.ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteFullEndElement();
                writer.Flush();
                writer.Close();
            }
        }
        public void Save(string ScriptLocation)
        {
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "XML file (*.xml)|*.xml";
            System.Windows.Forms.DialogResult result = save.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(save.FileName, settings);
                writer.WriteStartElement("Map");
                writer.WriteElementString("Width", MapWidth.ToString());
                writer.WriteElementString("Height", MapHeight.ToString());
                writer.WriteElementString("Format", "1");
                writer.WriteElementString("HasScript", "true");
                XmlReader scriptReader = XmlReader.Create(ScriptLocation);
                scriptReader.ReadToFollowing("Script");
                writer.WriteNode(scriptReader, true);
                foreach (Planet p in Planets)
                {
                    writer.WriteStartElement("Planet");

                    writer.WriteElementString("ID", p.ID.ToString());
                    writer.WriteElementString("Owner", p.Owner.ToString());
                    writer.WriteElementString("Forces", p.Forces.ToString());
                    writer.WriteElementString("Growth", p.Growth.ToString());
                    writer.WriteElementString("GrowthCooldown", p.GrowthCounter.ToString());
                    writer.WriteElementString("Size", p.PlanetSize.ToString());
                    writer.WriteElementString("HasPeople", p.HasPeople ? "true" : "false");

                    writer.WriteStartElement("Position");
                    writer.WriteElementString("X", p.Position.X.ToString());
                    writer.WriteElementString("Y", p.Position.Y.ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteFullEndElement();
                writer.Flush();
                writer.Close();
            }
        }
        public void Load()
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.CheckPathExists = true;
            open.CheckFileExists = true;
            open.AddExtension = true;
            open.Filter = "XML files (*.xml)|*.xml";
            System.Windows.Forms.DialogResult result = open.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Reset();
                Planets.AddRange(LoadMap(open.FileName));
                foreach (Planet p in Planets)
                {
                    if (p.ID > NextID)
                        NextID = p.ID;
                }
            }
        }

        private IEnumerable<Planet> LoadMap(string map)
        {
            Chance = MathHelper.Lerp(0.1f,0.6f,(float)rnd.NextDouble());
            List<Planet> mapPlanets = new List<Planet>();
            XmlReader xmlReader = XmlReader.Create(map);
            xmlReader.ReadStartElement();

            //xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("Width");
            int Width = xmlReader.ReadContentAsInt();

            xmlReader.Read();
            xmlReader.MoveToElement();
            xmlReader.ReadStartElement("Height");
            int Height = xmlReader.ReadContentAsInt();

            MapWidth = Width;
            MapHeight = Height;
            RefreshLine();
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
                while (xmlReader.Read())
                {
                    if (xmlReader.LocalName == "Script" && !xmlReader.IsStartElement())
                    {
                        break;
                    }
                }
            }
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
                    //xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("X");
                    Position.X = xmlReader.ReadContentAsInt();

                    /*Read the Y element*/
                    xmlReader.Read();
                    xmlReader.MoveToElement();
                    xmlReader.ReadStartElement("Y");
                    Position.Y = xmlReader.ReadContentAsInt();

                    Planet p = new Planet();
                    p.ID = ID;
                    p.Position = Position;
                    p.Growth = growth;
                    p.GrowthCounter = growthcd;
                    p.Owner = (PlayerType)Enum.Parse(typeof(PlayerType), owner, false);
                    p.Forces = forces;
                    p.PlanetSize = size;
                    p.HasPeople = hasppl;
                    if (defaultConnection != null && !defaultConnection.Dead)
                    {
                        string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", p.ID, p.Position.X, p.Position.Y, p.PlanetSize, p.Forces, (int)p.Owner, p.GrowthCounter, p.Growth);
                        defaultConnection.Write(Command);
                    }
                    mapPlanets.Add(p);
                }

            }
            xmlReader.Close();
            xmlReader = null;
            return mapPlanets;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameFont = Content.Load<SpriteFont>("GameFont");
            PlanetTexture = Content.Load<Texture2D>("Planet");
            Planets = new List<Planet>();
            Reset();
            // TODO: use this.Content to load your game content here
        }
        public void Reset()
        {
            lock (Planets)
                Planets.Clear();
            if (defaultConnection != null && !defaultConnection.Dead)
            {
                string Command = string.Format("+CLEAR\n");
                defaultConnection.Write(Command);
            }
            MapWidth = 800;
            MapHeight = 480;
            NextID = 0;
            Camera = new Camera();
            Camera.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera.Zoom = 1f;
            Camera.Rotation = 0f;
            Chance = MathHelper.Lerp(0.1f, 0.6f, (float)rnd.NextDouble());
            RefreshLine();
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        public void ApplySizeChange(int Width, int Height)
        {
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            graphics.ApplyChanges();
            //Camera = new Camera();
            //Camera.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            //Camera.Zoom = 1f;
            //Camera.Rotation = 0f;
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            remoteWorker.Heartbeat();
            MouseState NewState = Mouse.GetState();
            Vector2 MousePosition=new Vector2(NewState.X,NewState.Y)/Camera.Zoom-new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2)+Camera.Position;
            if (NewState.LeftButton == ButtonState.Pressed && OldState.LeftButton == ButtonState.Released)
            {
                lock (Planets)
                {
                    List<Planet> Selections = new List<Planet>(from P in Planets where (Vector2.Distance(P.Position, MousePosition) <= 64 * P.PlanetSize) orderby Vector2.Distance(P.Position, MousePosition) ascending select P);

                    if (Selections.Count > 0)
                    {
                        Selected = Selections[0];
                        SelectionChanged(this, EventArgs.Empty);
                    }
                }
            }
            else if (NewState.LeftButton == ButtonState.Pressed && OldState.LeftButton == ButtonState.Pressed)
            {
                if (Selected != null)
                {
                    Selected.Position = MousePosition;
                    if (defaultConnection != null && !defaultConnection.Dead)
                    {
                        string Command = string.Format("+UPDATE {0} {1} {2} {3} {4} {5} {6} {7}\n", Selected.ID, Selected.Position.X, Selected.Position.Y, Selected.PlanetSize, Selected.Forces, (int)Selected.Owner, Selected.GrowthCounter, Selected.Growth);
                        defaultConnection.Write(Command);
                    }
                }
            }
            else if(NewState.LeftButton==ButtonState.Released && OldState.LeftButton==ButtonState.Pressed)
            {
                if (Selected != null)
                {
                    if (defaultConnection != null && !defaultConnection.Dead)
                    {
                        string Command = string.Format("+UPDATE {0} {1} {2} {3} {4} {5} {6} {7}\n", Selected.ID, Selected.Position.X, Selected.Position.Y, Selected.PlanetSize, Selected.Forces, (int)Selected.Owner, Selected.GrowthCounter, Selected.Growth);
                        defaultConnection.Write(Command);
                    }
                }
                Selected=null;
            }
            if (NewState.RightButton == ButtonState.Pressed)
            {
                Camera.Position +=  new Vector2(OldState.X, OldState.Y)-new Vector2(NewState.X, NewState.Y);
            }
            OldState = NewState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,null,Camera.GetViewMatrix(Vector2.One,GraphicsDevice));
            lock (Planets)
                foreach (Planet planet in Planets)
                {
                    Color DrawColor = Color.White;
                    if (planet.Owner == PlayerType.Player1)
                    {
                        DrawColor = Color.LightGreen;
                    }
                    else if (planet.Owner == PlayerType.Player2)
                    {
                        DrawColor = Color.Red;
                    }
                    spriteBatch.Draw(PlanetTexture, planet.Position, null, DrawColor, 0f, new Vector2(PlanetTexture.Width / 2, PlanetTexture.Height / 2), planet.PlanetSize, SpriteEffects.None, 0f);
                    Vector2 TextCenter = GameFont.MeasureString(planet.Forces.ToString()) / 2;
                    spriteBatch.DrawString(GameFont, planet.Forces.ToString(), planet.Position, Color.White, 0f, TextCenter, 1f, SpriteEffects.None, 0f);
                }
            Line.Render(spriteBatch);
            spriteBatch.End();
            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        
    }
}
