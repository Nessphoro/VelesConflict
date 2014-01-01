using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VelesConflict.Gameplay
{
    class Campaign
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Epilogue { get; set; }
        public List<Episode> Episodes { get; set; }


        public void Load(XmlReader reader)
        {
            Campaign parsed=this;
            parsed.Episodes = new List<Episode>();
            reader.ReadToFollowing("Campaign");
            reader.ReadToFollowing("Name");
            parsed.Name = reader.ReadElementContentAsString();
            reader.ReadToFollowing("InternalName");
            parsed.InternalName = reader.ReadElementContentAsString();
            reader.ReadToFollowing("ShortDescription");
            parsed.ShortDescription = reader.ReadElementContentAsString();

            reader.ReadToFollowing("LongDescription");
            parsed.LongDescription = reader.ReadElementContentAsString();
            reader.ReadToFollowing("Epilogue");
            parsed.Epilogue = reader.ReadElementContentAsString();
            reader.ReadToFollowing("Gameplay");

            while (reader.Read())
            {
                if (reader.IsStartElement("Episode"))
                {
                    Episode episode = new Episode();
                    episode.Missions = new List<Mission>();
                    parsed.Episodes.Add(episode);
                    reader.ReadToFollowing("Name");
                    reader.Read();
                    episode.Name = reader.ReadContentAsString();

                    

                    reader.ReadToFollowing("Description");
                    episode.Description = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("EpisodePopup");
                    episode.EpisodePopup = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Position");
                    episode.Position = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Cells");
                    reader.Read();
                    string Cells = reader.ReadContentAsString();
                    episode.Cells=(from cell in Cells.Split(',') select int.Parse(cell)).ToArray();
                    reader.ReadToFollowing("Missions");

                    while (reader.Read())
                    {
                        if (reader.IsStartElement("Mission"))
                        {
                            Mission mission = new Mission();
                            episode.Missions.Add(mission);
                            reader.ReadToFollowing("Name");
                            reader.Read();
                            mission.Name = reader.ReadContentAsString();
                            reader.ReadToFollowing("PointsGain");
                            mission.PointsGain = reader.ReadElementContentAsInt();
                            reader.ReadToFollowing("Map");
                            reader.Read();
                            mission.Map = reader.ReadContentAsString();

                            reader.ReadToFollowing("Player1TexturePack");
                            reader.Read();
                            mission.Player1TexturePack = reader.ReadContentAsString();

                            reader.ReadToFollowing("Player2TexturePack");
                            reader.Read();
                            mission.Player2TexturePack = reader.ReadContentAsString();

                            reader.ReadToFollowing("Description");
                            reader.Read();
                            mission.Description = reader.ReadContentAsString();

                            reader.ReadToFollowing("Popup");
                            reader.Read();
                            mission.Popup = reader.ReadContentAsString();
                        }
                        else if (reader.LocalName == "Episode" && !reader.IsStartElement())
                        {
                            break;
                        }
                    }
                }

            }
        }
    }
}
