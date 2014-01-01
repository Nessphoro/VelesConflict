using Atom.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelesNodes
{
    [Node("Planet", "Veles")]
    public sealed class OnScriptLoadedNode : AtomNode
    {
        public OnScriptLoadedNode(AtomScript Script)
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
        }
    }
    [Node("Map", "Veles")]
    public sealed class MapInfo : AtomNode
    {
        public MapInfo(AtomScript Script)
            : base(Script)
        {
            this.Inputs.Add("Get", new Input(this, ConnectionType.Void));

            this.Outputs.Add("Name", new Output(this, ConnectionType.Int));
            this.Outputs.Add("TotalPlanets", new Output(this, ConnectionType.Int));
        }
    }
    [Node("GetPlanets", "Veles")]
    public sealed class GetPlanets : AtomNode
    {
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
