using Atom.Phone.TCP;
using Atom.Shared.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using VelesConflict.Gameplay.Multiplayer;

namespace VelesConflict.Shared
{
    enum SelectionState
    {
        Selection, Target
    }
    class MultiplayerBase
    {
        enum MultiplayerGameState
        {
            Connecting, Waiting, Downloading, Game, Ready, Queue
        }
        VelesConflict.Gameplay.PlayerType Player;
        SelectionState SelectionState;
        MultiplayerGameState GameState;
        SpriteBatch spriteBatch;
        bool Syncing = true;
        MultiplayerGameManager Manager;
        private int GameCounter;
        private Client client;
        private int FramesToSpin;
        private Packet SendF;
        List<Planet> Selected = new List<Planet>();
        WebClient WebClient;


        Texture2D Fleet;
        ContentManager Content;

        private int Map;
        IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
        IInputServiceContract contract;

        bool Destroyed = false;
        public MultiplayerBase(string address, int port, PGGI.PGGIProvider provider)
        {
            ISpriteBatchService spriteService = Atom.Shared.Globals.Engine.GetService<ISpriteBatchService>();
            IInputService inputService = Atom.Shared.Globals.Engine.GetService<IInputService>();

            contract = inputService.CreateContract();
            contract.Enabled = true;
            contract.SubscribedTouchEvents = TouchStates.OnDoubleTap | TouchStates.OnDrag | TouchStates.OnTap;
            contract.TouchEventHandler += contract_TouchEventHandler;

            spriteBatch = spriteService.SpriteBatch;

            SelectionState = SelectionState.Selection;
            GameState = MultiplayerGameState.Connecting;
            GameCounter = 0;
            client = new Client();
            client.OnConnection += new EventHandler<ConnectionArgs>(ClientOnConnection);
            client.OnPacketRecieved += new EventHandler<PacketRecievedArgs>(ClientOnPacket);
            Packet ConnectionPacket = new Packet();
            ConnectionPacket.Write(provider.PGGI);
            ConnectionPacket.Write(provider.Token);
            client.Connect(new System.Net.DnsEndPoint(address, port), ConnectionPacket);
            Syncing = false;
            FramesToSpin = int.MaxValue;
            Manager = new MultiplayerGameManager();
            Content = new ContentManager(Atom.Shared.Globals.Engine, "Content");
           
            Fleet = Content.Load<Texture2D>("Graphics/Fleets/1");
        }

        void contract_TouchEventHandler(object sender, TouchEventArgs e)
        {

            Vector2 ActualLocation = e.Location;

            if (e.State == TouchStates.OnDrag || e.State == TouchStates.OnTap)
            {

                foreach (Planet planet in Manager.LeadingState.GetMyPlanets(Player))
                {
                    float Range = planet.PlanetSize * 64 + 32;
                    if (Vector2.Distance(planet.Position, ActualLocation) <= Range)
                    {

                        if (!Selected.Any(p => p.Id == planet.Id))
                        {
                            Selected.Add(planet);
                        }
                    }

                }
            }
            else if (e.State == TouchStates.OnDoubleTap)
            {
                Planet Target = null;
                foreach (Planet planet in Manager.LeadingState.Planets)
                {
                    float Range = planet.PlanetSize * 64 + 32;
                    if (Vector2.Distance(planet.Position, ActualLocation) <= Range)
                    {
                        Target = planet;
                        break;
                    }
                }
                if (Target != null)
                {
                    List<Planet> Sanitized = new List<Planet>();
                    Sanitized.AddRange(from RealPlanet in Selected where RealPlanet.Id != Target.Id select Manager.LeadingState.Planets.First(p => p.Id == RealPlanet.Id));


                    if (Sanitized.Count > 0)
                    {
                        SendF = new Packet();
                        SendF.Write((byte)0);
                        SendF.Write(Player == VelesConflict.Gameplay.PlayerType.Player1 ? 1 : 2);
                        SendF.Write(Manager.LeadingState.StateFrame);
                        SendF.Write(Sanitized.Count);
                    }
                    foreach (Planet selected in Sanitized)
                    {
                        if (selected.Owner == Player)
                        {
                            SendFleet(selected, Target);
                            lock (Manager)
                            {
                                Manager.SendFleet(0, selected, Target, Manager.LeadingState.StateFrame);
                            }
                        }
                    }
                }
                Selected.Clear();
            }
        }
        


        protected void ClientOnPacket(object sender, PacketRecievedArgs e)
        {
            e.Drop = true;
            byte Type = e.Packet.ReadByte();

            switch (Type)
            {
                case 0:
                    int Owner = e.Packet.ReadInt();
                    int SimulationTick = e.Packet.ReadInt();
                    int Count = e.Packet.ReadInt();
                    for (int i = 0; i < Count; i++)
                    {

                        int OriginID = e.Packet.ReadInt();
                        int DestinationID = e.Packet.ReadInt();
                        SendRequest r = new SendRequest();
                        r.SimulationTick = SimulationTick;
                        r.Owner = Owner == 1 ? VelesConflict.Gameplay.PlayerType.Player1 : VelesConflict.Gameplay.PlayerType.Player2;
                        r.Origin = OriginID;
                        r.Destination = DestinationID;
                        Manager.QueueAction(r);
                    }
                    break;
                case 0xFB:
                    GameState = MultiplayerGameState.Waiting;
                    KillGame(true);
                    
                    break;
                case 0xFD:
                    GameState = MultiplayerGameState.Downloading;
                    Map = e.Packet.ReadInt();
                    if (!storage.DirectoryExists("MultiplayerMaps"))
                    {
                        storage.CreateDirectory("MultiplayerMaps");
                    }
                    if (storage.FileExists(string.Format(@"MultiplayerMaps\map{0}", Map)))
                    {
                        List<Planet> planets = (LoadMap(string.Format(@"MultiplayerMaps\map{0}", Map)));
                        foreach (Planet p in planets)
                            Manager.AddPlanet(p);
                        GameState = MultiplayerGameState.Ready;
                        Thread.Sleep(1000);
                        Packet ready = new Packet();
                        ready.Write((byte)0xFE);
                        client.SendPacket(ready);
                    }
                    else
                    {
                        string MapAddress = string.Format(@"http://www.velesconflict.com/maps/map{0}.xml", Map);
                        WebClient = new WebClient();
                        WebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(WebClient_OpenReadCompleted);
                        WebClient.OpenReadAsync(new Uri(MapAddress));
                    }
                    break;
                case 0xFC:
                    Player = (VelesConflict.Gameplay.PlayerType)e.Packet.ReadByte();
                    break;
                case 0xFF:
                    GameCounter = 0;
                    GameState = MultiplayerGameState.Game;
                    Syncing = true;
                    FramesToSpin = 120 - (client.Ping() / 2 + 17 / 2) / 17;
                    break;
            }

            e.Packet.ResetPointer();

        }
        protected void ClientOnConnection(object sender, ConnectionArgs e)
        {
            byte Type = e.Packet.ReadByte();
            switch (Type)
            {
                case 0xFC:
                    GameState = MultiplayerGameState.Waiting;
                    byte pType = e.Packet.ReadByte();
                    Player = pType == 1 ? VelesConflict.Gameplay.PlayerType.Player1 : VelesConflict.Gameplay.PlayerType.Player2;
                    break;
            }
        }

        void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                IsolatedStorageFileStream fs = new IsolatedStorageFileStream(string.Format(@"MultiplayerMaps\map{0}", Map), System.IO.FileMode.Create, storage);
                e.Result.CopyTo(fs);
                fs.Flush();
                fs.Dispose();
                e.Result.Dispose();
                List<Planet> planets = (LoadMap(string.Format(@"MultiplayerMaps\map{0}", Map)));
                foreach (Planet p in planets)
                    Manager.AddPlanet(p);
                GameState = MultiplayerGameState.Ready;
                Thread.Sleep(1000);
                Packet ready = new Packet();
                ready.Write((byte)0xFE);
                client.SendPacket(ready);
            }
        }

        private List<Planet> LoadMap(string map)
        {
            List<Planet> mapPlanets = new List<Planet>();
            IsolatedStorageFileStream fs = new IsolatedStorageFileStream(map, System.IO.FileMode.Open, storage);


            int LargeNotPopulated = 3;
            int LargePopulated = 4;

            int MediumNotPopulated = 2;
            int MediumPopulated = 3;

            int SmallNotPopulated = 3;
            int SmallPopulated = 3;

            Random rnd = new Random();

            XmlReader xmlReader = XmlReader.Create(fs);
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
                    p.Owner = (VelesConflict.Gameplay.PlayerType)Enum.Parse(typeof(VelesConflict.Gameplay.PlayerType), owner, false);
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
                        if (rnd.NextDouble() < 0.35)
                            pz = PlanetSize.Small;
                        else
                            pz = PlanetSize.Medium;
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


                    p.Texture = Content.Load<Texture2D>(TextureLocation);
                    mapPlanets.Add(p);
                }

            }
            fs.Close();
            return mapPlanets;
        }

        private void KillGame(bool final)
        {
            lock (this)
            {
                if (final)
                {
                    client.Disconnect();
                }
                if (!Destroyed)
                {
                    Destroyed = true;
                    IInputService inputService = Atom.Shared.Globals.Engine.GetService<IInputService>();
                    inputService.DestroyContract(contract);


                    Content.Dispose();
                    GameBase.Multiplayer = null;
                    (Atom.Shared.Globals.Engine as GameBase).EnableMenues();
                    GameBase.GameState = VelesConflict.Shared.GameState.Menu;
                }
            }
        }




        internal void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (GameState != MultiplayerGameState.Game)
                return;
            GameCounter++;
            if (Syncing)
            {
                if (GameCounter >= FramesToSpin)
                {
                    Syncing = false;
                    GameCounter = 0;
                    return;
                }
            }
            else
            {
                if(Manager.GameEnd())
                {
                    KillGame(false);
                }
                foreach (Planet p in Manager.LeadingState.Planets)
                {
                    if (Selected.Any(pred => pred.Id == p.Id))
                    {
                        Selected.First(f => f.Id == p.Id).SelectionRotation += 0.05f;
                    }
                    else
                        p.SelectionRotation = 0;
                }
                
                #region Input
                /*
                TouchCollection touchCollection = TouchPanel.GetState();
                foreach (TouchLocation tl in touchCollection)
                {
                    if (tl.State == TouchLocationState.Moved)
                    {
                        if (SelectionState == SelectionState.Selection)
                        {
                            foreach (Planet planet in Manager.LeadingState.Planets)
                            {
                                if (planet.Owner == Player && Vector2.Distance(tl.Position, planet.Position) < (planet.PlanetSize < 0.5 ? planet.PlanetSize + 0.2 : planet.PlanetSize) * 64)
                                {
                                    if (Selected.Contains(planet))
                                        continue;
                                    Selected.Add(planet);
                                    break;
                                }
                            }
                        }
                    }
                    else if (tl.State == TouchLocationState.Released)
                    {
                        if (SelectionState == SelectionState.Selection)
                        {
                            SelectionState = SelectionState.Target;
                            continue;
                        }

                        foreach (Planet planet in Manager.LeadingState.Planets)
                        {
                            if (Vector2.Distance(tl.Position, planet.Position) < (planet.PlanetSize < 0.5 ? planet.PlanetSize + 0.2 : planet.PlanetSize) * 64 && Selected.Count > 0)
                            {
                                List<Planet> Sanitized = new List<Planet>();
                                Sanitized.AddRange(from RealPlanet in Selected where RealPlanet.Id != planet.Id select RealPlanet);


                                if (Sanitized.Count > 0)
                                {
                                    SendF = new Packet();
                                    SendF.Write((byte)0);
                                    SendF.Write(Player == VelesConflict.Gameplay.PlayerType.Player1 ? 1 : 2);
                                    SendF.Write(Manager.LeadingState.StateFrame);
                                    SendF.Write(Sanitized.Count);
                                }
                                foreach (Planet selected in Sanitized)
                                {
                                    //if (selected.Owner == PlayerType.Player)
                                    //{
                                    SendFleet(selected, planet);
                                    lock (Manager)
                                    {
                                        Manager.SendFleet(0, selected, planet, Manager.LeadingState.StateFrame);
                                    }
                                }
                                //}
                                break;
                            }

                        }
                        SelectionState = SelectionState.Selection;
                        Selected.Clear();
                    }

                }
                */
                #endregion

                if (SendF != null)
                {
                    client.SendPacket(SendF);
                    SendF = null;
                }

                Manager.Update();

                List<Planet> NotOwned = new List<Planet>();
                foreach (Planet p in Selected)
                {
                    if (p.Owner != Player)
                        NotOwned.Add(p);
                }
                foreach (Planet p in NotOwned)
                {
                    Selected.Remove(p);
                }
            }
        }

        private void SendFleet(Planet origin, Planet destination)
        {
            if (origin.Owner != Player)
                return;
            if (origin.Id == destination.Id)
                return;
            SendF.Write(origin.Id);
            SendF.Write(destination.Id);
        }

        public void Draw()
        {
            spriteBatch.Begin();
            if (GameState != MultiplayerGameState.Game || Syncing)
            {
                spriteBatch.Draw(GameBase.Splash, Vector2.Zero, Color.White);
                return;
            }

            foreach (Planet planet in Manager.LeadingState.Planets)
            {
                Color DrawColor = Color.White;


                if (planet.Owner == Player)
                    DrawColor = Color.LightGreen;
                else if (planet.Owner == VelesConflict.Gameplay.PlayerType.Neutral)
                    DrawColor = Color.White;
                else
                    DrawColor = Color.DarkRed;

                spriteBatch.Draw(planet.Texture, planet.Position, null, DrawColor, 0f, GameBase.PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 1f);
                spriteBatch.DrawString(GameBase.Font, planet.Forces.ToString(), planet.Position, DrawColor);

            }
            if (Syncing)
            {
                return;
            }
            foreach (Fleet fleet in Manager.LeadingState.Fleets)
            {
                Color DrawColor = Color.White;

                if (fleet.Owner == Player)
                    DrawColor = Color.LightGreen;
                else
                    DrawColor = Color.DarkRed;

                spriteBatch.Draw(Fleet, fleet.Position, null, DrawColor, fleet.Rotation, GameBase.FleetOrigin, 0.25f, SpriteEffects.None, 0f);
                foreach (Vector2 p in fleet.Positions)
                {
                    spriteBatch.Draw(Fleet, p + fleet.Position, null, DrawColor, fleet.Rotation, GameBase.FleetOrigin, 0.25f, SpriteEffects.None, 0f);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (Planet planet in Manager.LeadingState.Planets)
            {
                if (Selected.Contains(planet))
                {
                    spriteBatch.Draw(planet.Texture, planet.Position, null, Color.IndianRed, 0f, GameBase.PlanetOrigin, planet.PlanetSize, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
