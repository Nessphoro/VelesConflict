using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atom.Graph;
using Microsoft.Xna.Framework;
using Atom.Shared.Services;
using Atom.Shared;
using VelesConflict.Shared;

namespace VelesConflict.Gameplay
{
    
    [Node("Planet", "Veles")]
    public sealed class PlanetNode : AtomNode
    {
        Planet Planet = null;
        int ForcesPrevious = 0;
        public PlanetNode(AtomScript Script)
            : base(Script)
        {
            this.Inputs.Add("ID", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Owner", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Forces", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Position", new Input(this, ConnectionType.Vector2));
            this.Inputs.Add("Load", new Input(this, ConnectionType.Void));

            this.Outputs.Add("OwnerChanged", new Output(this, ConnectionType.Int));
            this.Outputs.Add("Owner", new Output(this, ConnectionType.Int));
            this.Outputs.Add("Forces", new Output(this, ConnectionType.Int));
            this.Outputs.Add("Position", new Output(this, ConnectionType.Vector2));

            this.Inputs["Load"].OnInput += OnLoad;
            this.Inputs["Forces"].OnInput += OnSetForces;
            this.Inputs["Owner"].OnInput += OnSetOwner;
            this.Inputs["Position"].OnInput += OnSetPosition;


            this.Outputs["Owner"].OnBackpropagate += OwnerBack;
            this.Outputs["Forces"].OnBackpropagate += ForcesBack;
            this.Outputs["Position"].OnBackpropagate += PositionBack;
            IUpdateService service = Globals.Engine.GetService(typeof(IUpdateService)) as IUpdateService;
            service.Subscribe(OnUpdateEvent);
        }

        private void OnUpdateEvent(GameTime gameTime)
        {
            if (this.Script.Loaded)
            {
                if(Planet!=null)
                if (ForcesPrevious != Planet.Forces)
                {
                    this.Outputs["Forces"].Call((int)Planet.Forces);
                    ForcesPrevious = Planet.Forces;
                }
            }
        }

        void PositionBack(object sender, OnBackpropagate e)
        {
            if (Planet != null)
            {
                e.Data = Planet.Position;
            }
            else
                e.Data = Vector2.Zero;
        }
        void ForcesBack(object sender, OnBackpropagate e)
        {
            if (Planet != null)
            {
                e.Data = Planet.Forces;
            }
            else
                e.Data = 0;
        }
        void OwnerBack(object sender, OnBackpropagate e)
        {
            if (Planet != null)
            {
                e.Data = (int)Planet.Owner;
            }
            else
                e.Data = 0;
        }

        void OnCaptured()
        {
            this.Outputs["Owner"].Call((int)Planet.Owner);
            this.Outputs["OwnerChanged"].Call((int)Planet.Owner);
        }
        void OnLoad(object sender, OnInput e)
        {
            int NewID = (int)this.Inputs["ID"].Data;
            if (Planet != null)
            {
                Planet.Captured -= OnCaptured;
            }
            Planet = (from P in VelesConflict.Shared.GameBase.GameManager.State.Planets where P.Id == NewID select P).First();
            Planet.Captured += OnCaptured;

            this.Outputs["Forces"].Call((int)Planet.Forces);
            this.Outputs["Owner"].Call((int)Planet.Owner);
            this.Outputs["Position"].Call(Planet.Position);
            ForcesPrevious = Planet.Forces;
        }

        void OnSetForces(object sender, OnInput e)
        {
            if (Planet != null)
            {
                Planet.Forces = (int)e.Data;
            }
        }
        void OnSetOwner(object sender, OnInput e)
        {
            if (Planet != null)
            {
                Planet.Owner = (PlayerType)e.Data;
            }
        }
        void OnSetPosition(object sender, OnInput e)
        {
            if (Planet != null)
            {
                Planet.Position = (Vector2)e.Data;
            }
        }
    }
    [Node("Send Fleet", "Veles")]
    public sealed class SendFleet : AtomNode
    {
        public SendFleet(AtomScript Script)
            : base(Script)
        {
            this.Inputs.Add("Forces", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Owner", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Position", new Input(this, ConnectionType.Vector2));
            this.Inputs.Add("Source", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Distination", new Input(this, ConnectionType.Int));
            this.Inputs.Add("Send", new Input(this, ConnectionType.Void));

            this.Outputs.Add("Done", new Output(this, ConnectionType.Void));
            this.Inputs["Send"].OnInput += OnSend;
            this.Inputs["Owner"].Data = 1;
        }

        void OnSend(object sender, OnInput e)
        {
            int Forces = (int)this.Inputs["Forces"].Data;
            Vector2 Position = (Vector2)this.Inputs["Position"].Data;
            int Destination = (int)this.Inputs["Distination"].Data;
            Planet DestinationPlanet = null;
            foreach (Planet planet in VelesConflict.Shared.GameBase.GameManager.State.Planets)
            {
                if (planet.Id == Destination)
                    DestinationPlanet = planet;
            }
            VelesConflict.Shared.GameBase.GameManager.SendFleet(Forces,(PlayerType)(int)this.Inputs["Owner"].Data, Position, DestinationPlanet);
            this.Outputs["Done"].Call(null);
        }
    }
    [Node("Game", "Veles")]
    public sealed class VelesGame : AtomNode
    {
        public VelesGame(AtomScript Script)
            : base(Script)
        {
            this.Inputs.Add("Trigger Victory", new Input(this, ConnectionType.Void));
            this.Inputs.Add("Trigger Defeat", new Input(this, ConnectionType.Void));
        }
    }
    [Node("GetPlanets", "Veles")]
    public sealed class GetPlanets : AtomNode
    {
        List<Planet> list = null;
        int counter = 0;
        public GetPlanets(AtomScript script)
            : base(script)
        {
            this.Inputs.Add("Player", new Input(this, ConnectionType.Int));
            this.Inputs.Add("IncludeMy", new Input(this, ConnectionType.Bool));
            this.Inputs.Add("IncludeEnemy", new Input(this, ConnectionType.Bool));
            this.Inputs.Add("IncludeNeutral", new Input(this, ConnectionType.Bool));
            this.Inputs.Add("Get", new Input(this, ConnectionType.Void));
            this.Inputs.Add("Next", new Input(this, ConnectionType.Void));
            this.Outputs.Add("For Each", new Output(this, ConnectionType.Int));

            this.Inputs["Get"].OnInput += new EventHandler<OnInput>(GetPlanets_OnInput);
            this.Inputs["Next"].OnInput += new EventHandler<OnInput>(OnNext);
        }

        void OnNext(object sender, OnInput e)
        {
            if (counter < list.Count)
            {
                this.Outputs["For Each"].Call(list[counter++].Id);
            }
            else
                counter = 0;
        }

        void GetPlanets_OnInput(object sender, OnInput e)
        {
            if (counter != 0)
                return;
            PlayerType player = (PlayerType)(int)this.Inputs["Player"].Data;
            list = new List<Planet>();
            bool includeMy = (bool)this.Inputs["IncludeMy"].Data;
            bool includeEnemy = (bool)this.Inputs["IncludeEnemy"].Data;
            bool includeNeutral = (bool)this.Inputs["IncludeNeutral"].Data;
            counter = 0;
            if (includeMy)
            {
                list.AddRange(from p in GameBase.GameManager.State.Planets where p.Owner == player select p);
            }
            if (includeEnemy)
            {
                list.AddRange(from p in GameBase.GameManager.State.Planets where p.Owner != player && p.Owner != PlayerType.Neutral select p);
            }
            if (includeNeutral)
            {
                list.AddRange(from p in GameBase.GameManager.State.Planets where p.Owner == PlayerType.Neutral select p);
            }
            if (list.Count > 0)
            {
                this.Outputs["For Each"].Call(list[0].Id);
                counter = 1;
            }
        }
    }

    [Node("Map", "Veles")]
    public sealed class MapInfo : AtomNode
    {
        
        [NodeOutput]
        public int TotalPlanets
        {
            set
            {
                Outputs["TotalPlanets"].Call(value);
            }
        }

        
        [NodeOutput]
        public string Name
        {
            set
            {
                Outputs["Name"].Call(value);
            }
        }
                
        

        public MapInfo(AtomScript Script)
            : base(Script)
        {
            Outputs["Name"].OnBackpropagate += MapInfo_OnBackpropagate;
            Outputs["TotalPlanets"].OnBackpropagate += TotalPlanets_OnBackpropagate;
            Script.OnLoad += Script_OnLoad;
        }

        void Script_OnLoad(object sender, EventArgs e)
        {
            Get();
        }

        void TotalPlanets_OnBackpropagate(object sender, OnBackpropagate e)
        {
            e.Data = GameBase.GameManager.MapName;
        }

        void MapInfo_OnBackpropagate(object sender, OnBackpropagate e)
        {
            e.Data = GameBase.GameManager.State.Planets.Count;
        }

        [NodeInput]
        public void Get()
        {
            Name = GameBase.GameManager.MapName;
            TotalPlanets = GameBase.GameManager.State.Planets.Count;
        }
    }

    [Node("Achievements", "Veles")]
    public sealed class VelesAchievements : AtomNode
    {
        public VelesAchievements(AtomScript Script)
            : base(Script)
        {
            this.Inputs.Add("Achievement", new Input(this, ConnectionType.String));
            this.Inputs.Add("Trigger", new Input(this, ConnectionType.Void));
        }
    }
}
