using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelesConflict.Gameplay
{
    class Mission
    {
        public string Name { get; set; }
        public int PointsGain { get; set; }
        public string Map { get; set; }
        public string Player1TexturePack { get; set; }
        public string Player2TexturePack { get; set; }
        public string Description { get; set; }
        public string Popup { get; set; }
    }
}
