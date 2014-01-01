using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Gameplay
{
    class Episode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string EpisodePopup { get; set; }
        public int Position { get; set; }
        public int[] Cells { get; set; }
        public List<Mission> Missions { get; set; }
    }
}
