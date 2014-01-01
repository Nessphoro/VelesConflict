using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using VelesConflict.Shared;

namespace VelesConflict.Gameplay
{
    public enum PlayerType:int
    {
        Player1=1,Player2=2,Neutral=0
    }
    public class MapState
    {
        public List<Planet> Planets
        {
            get;
            set;
        }
        public List<Fleet> Fleets
        {
            get;
            set;
        }

        public List<Planet> GetMyPlanets(PlayerType type)
        {
            List<Planet> ret = new List<Planet>();
            foreach (Planet p in Planets)
            {
                if (p.Owner == type)
                {
                    ret.Add(p);
                }
            }
            return ret;
        }
        public List<Planet> GetEnemyPlanets(PlayerType type)
        {
            List<Planet> ret = new List<Planet>();
            foreach (Planet p in Planets)
            {
                if (p.Owner != type)
                {
                    ret.Add(p);
                }
            }
            return ret;
        }

        public List<Planet> PlayerPlanets
        {
            get
            {
                List<Planet> ret=new List<Planet>();
                foreach (Planet p in Planets)
                {
                    if (p.Owner == PlayerType.Player1)
                    {
                        ret.Add(p);
                    }
                }
                return ret;
            }
        }
        public List<Planet> AIPlanets
        {
            get
            {
                List<Planet> ret = new List<Planet>();
                foreach (Planet p in Planets)
                {
                    if (p.Owner == PlayerType.Player2)
                    {
                        ret.Add(p);
                    }
                }
                return ret;
            }
        }
        public List<Planet> NeutralPlanets
        {
            get
            {
                List<Planet> ret = new List<Planet>();
                foreach (Planet p in Planets)
                {
                    if (p.Owner == PlayerType.Neutral)
                    {
                        ret.Add(p);
                    }
                }
                return ret;
            }
        }
    }
    public class GameManager
    {
        public MapState State
        {
            get;
            set;
        }
        public string MapName;
        Random rnd=new Random();
        public float PlayerAttackBias = 1f;
        public float AIAttackBias = 1f;

        public GameManager()
        {
            State = new MapState();
            State.Fleets = new List<Fleet>(300);
            State.Planets = new List<Planet>(50);
        }

        public void SendFleet(int Ammount, Planet Origin, Planet Destination)
        {
            if (Origin.Id == Destination.Id)
                return;
            if (Origin.Forces < Ammount)
                return;
            Origin.Forces -= Ammount;

            Fleet fleet = new Fleet();
            fleet.Owner = Origin.Owner;
            //Put the ship on the edge
            Vector2 Normalized = Destination.Position - Origin.Position;
            Normalized.Normalize();
            fleet.Rotation = (float)Math.Atan2(Normalized.Y, Normalized.X) + MathHelper.ToRadians(90);
            Normalized *= Origin.PlanetSize * 64;

            fleet.Position = Origin.Position+Normalized;
            fleet.Origin = Origin;
            fleet.Destination = Destination;
            fleet.Count = Ammount;

            int ExtraShipCount = 0;
            if (fleet.Count-10 >= 10)
                ExtraShipCount = (fleet.Count-10) / 10;
            fleet.Positions = new Vector2[ExtraShipCount];
            Normalized.Normalize();

            for (int i = 0; i < ExtraShipCount; i++)
            {
                float Angle = MathHelper.ToRadians((float)(rnd.Next(360) + rnd.NextDouble()));
                float Distance=8+(float)Math.Sqrt(rnd.NextDouble())*((1+(float)Math.Sqrt(ExtraShipCount)/2)*8);
                fleet.Positions[i] =  new Vector2((float)Math.Cos(Angle)*Distance,(float)Math.Sin(Angle)*Distance);

                foreach (Vector2 p in fleet.Positions)
                {
                    if (p == null||p==fleet.Positions[i])
                        continue;

                    if(Vector2.Distance(p,fleet.Positions[i])<16)
                    {
                        Angle = MathHelper.ToRadians((float)(rnd.Next(360) + rnd.NextDouble()));
                        Distance = 8 + (float)Math.Sqrt(rnd.NextDouble()) * ((1 + (float)Math.Sqrt(ExtraShipCount)/2) * 8);
                        fleet.Positions[i] = new Vector2((float)Math.Cos(Angle) * Distance, (float)Math.Sin(Angle) * Distance);
                    }
                }
            }

            State.Fleets.Add(fleet);
        }
        public void SendFleet(int Ammount,PlayerType Type, Vector2 Origin, Planet Destination)
        {
            Fleet fleet = new Fleet();
            fleet.Owner = Type;
            //Put the ship on the edge
            Vector2 Normalized = Destination.Position - Origin;
            Normalized.Normalize();
            fleet.Rotation = (float)Math.Atan2(Normalized.Y, Normalized.X) + MathHelper.ToRadians(90);

            fleet.Position = Origin;
            fleet.Origin = null;
            fleet.Destination = Destination;
            fleet.Count = Ammount;

            int ExtraShipCount = 0;
            if (fleet.Count - 10 >= 10)
                ExtraShipCount = (fleet.Count - 10) / 10;
            fleet.Positions = new Vector2[ExtraShipCount];
            Normalized.Normalize();

            for (int i = 0; i < ExtraShipCount; i++)
            {
                float Angle = MathHelper.ToRadians((float)(rnd.Next(360) + rnd.NextDouble()));
                float Distance = 8 + (float)Math.Sqrt(rnd.NextDouble()) * ((1 + (float)Math.Sqrt(ExtraShipCount) / 2) * 8);
                fleet.Positions[i] = new Vector2((float)Math.Cos(Angle) * Distance, (float)Math.Sin(Angle) * Distance);

                foreach (Vector2 p in fleet.Positions)
                {
                    if (p == null || p == fleet.Positions[i])
                        continue;

                    if (Vector2.Distance(p, fleet.Positions[i]) < 16)
                    {
                        Angle = MathHelper.ToRadians((float)(rnd.Next(360) + rnd.NextDouble()));
                        Distance = 8 + (float)Math.Sqrt(rnd.NextDouble()) * ((1 + (float)Math.Sqrt(ExtraShipCount) / 2) * 8);
                        fleet.Positions[i] = new Vector2((float)Math.Cos(Angle) * Distance, (float)Math.Sin(Angle) * Distance);
                    }
                }
            }

            State.Fleets.Add(fleet);
        }
        public void SendFleet(int Ammount, int OriginID, int DestinationID)
        {
            Planet Origin=null, Destination=null;
            foreach (Planet planet in State.Planets)
            {
                if (planet.Id == OriginID)
                    Origin = planet;
                else if (planet.Id == DestinationID)
                    Destination = planet;
            }
            if (Origin == null || Destination == null)
                return;
            SendFleet(Ammount, Origin, Destination);
        }
        public bool GameEnd()
        {
            bool ret = false;
            PlayerType looser=PlayerType.Neutral;
            if (State.PlayerPlanets.Count == 0)
            {
                looser = PlayerType.Player1;
                ret = true;
            }
            if (State.AIPlanets.Count == 0)
            {
                looser = PlayerType.Player2;
                ret = true;
            }
            if (looser != PlayerType.Neutral&&ret==true)
            {
                foreach (Fleet f in State.Fleets)
                {
                    if (f.Owner == looser)
                        ret = false;
                }
            }

            return ret;
        }
        public PlayerType GetLooser()
        {
            bool ret = false;
            PlayerType looser = PlayerType.Neutral;
            if (State.PlayerPlanets.Count == 0)
            {
                looser = PlayerType.Player1;
                ret = true;
            }
            if (State.AIPlanets.Count == 0)
            {
                looser = PlayerType.Player2;
                ret = true;
            }
            if (looser != PlayerType.Neutral && ret == true)
            {
                foreach (Fleet f in State.Fleets)
                {
                    if (f.Owner == looser)
                        ret = false;
                }
            }

            return looser;
        }
        public float GetActualResearchModifier(string research)
        {
            switch (research)
            {
                case "Growth":
                    if (GameBase.playerData.Research["Growth"] == 6)
                    {
                        return GetBaseModifier("Growth") + 0.15f;
                    }
                    else if (GameBase.playerData.Research["Speed"] == 6)
                    {
                        return GetBaseModifier("Growth") - 0.1f;
                    }
                    else if (GameBase.playerData.Research["Defense"] == 6)
                    {
                        return GetBaseModifier("Growth")-0.15f;
                    }
                    else
                    {
                        return GetBaseModifier("Growth");
                    }
                case "Speed":
                    if (GameBase.playerData.Research["Speed"] == 6)
                    {
                        return GetBaseModifier("Speed") + 0.20f;
                    }
                    else
                    {
                        return GetBaseModifier("Speed");
                    }
                case "Attack":
                    if (GameBase.playerData.Research["Attack"] == 6)
                    {
                        return GetBaseModifier("Attack") + 0.25f;
                    }
                    else if (GameBase.playerData.Research["Growth"] == 6)
                    {
                        return GetBaseModifier("Attack") - 0.07f;
                    }
                    else
                    {
                        return GetBaseModifier("Attack");
                    }
                case "Defense":
                    if (GameBase.playerData.Research["Defense"] == 6)
                    {
                        return GetBaseModifier("Defense") + 0.20f;
                    }
                    else if (GameBase.playerData.Research["Attack"] == 6)
                    {
                        return GetBaseModifier("Defense") - 0.1f;
                    }
                    else if (GameBase.playerData.Research["Growth"] == 6)
                    {
                        return GetBaseModifier("Defense") - 0.05f;
                    }
                    else
                    {
                        return GetBaseModifier("Defense");
                    }
                default:
                    return 0;
            }
        }
        public float GetBaseModifier(string research)
        {
            switch (research)
            {
                case "Growth":
                    return GameBase.playerData.Research["Growth"] * 0.1f;
                case "Speed":
                    return GameBase.playerData.Research["Speed"] * 0.07f;
                case "Attack":
                    return GameBase.playerData.Research["Attack"] * 0.08f;
                case "Defense":
                    return GameBase.playerData.Research["Defense"] * 0.1f;
                default:
                    return 0;
            }
        }
        List<Fleet> DeadFleets = new List<Fleet>();

        int DeadCounter = 0;
        public void Update()
        {
            
            //Process planets
            foreach (Planet planet in State.Planets)
            {
                
                //Process growth
                if (planet.Owner != PlayerType.Neutral)
                {
                    planet.GrowthCounter--;
                    planet.GrowthCounter--;
                    if (planet.GrowthCounter <= 0)
                    {
                        planet.Forces += planet.Growth;
                        float GrowthModifier = GetActualResearchModifier("Growth");
                        planet.GrowthCounter = planet.GrowthReset - (planet.Owner == PlayerType.Player1 ? (int)(planet.GrowthReset * GrowthModifier) : (int)(planet.GrowthReset * GrowthModifier));
                    }
                }

     

                    
                
            }

            
            foreach (Fleet fleet in State.Fleets)
            {
                if (fleet.Dead)
                    continue;
                if (Vector2.Distance(fleet.Position, fleet.Destination.Position) < fleet.Destination.PlanetSize*64)
                {
                    //We have arrived at a planet
                    if (fleet.Destination.Owner == PlayerType.Neutral && fleet.Destination.Forces < fleet.Count)
                    {
                        //If the owner is neutral capture/attack is instantanious; otherwise process in the next turn
                        fleet.Destination.Owner = fleet.Owner;
                        fleet.Destination.Forces=fleet.Count - fleet.Destination.Forces;
                        if (fleet.Destination.Captured != null)
                        {
                            fleet.Destination.Captured();
                        }
                    }
                    else if (fleet.Destination.Owner == PlayerType.Neutral)
                    {
                        fleet.Destination.Forces -= fleet.Count;
                        if (fleet.Destination.Forces == 0)
                        {
                            fleet.Destination.Owner = fleet.Owner;
                        }
                    }
                    else
                    {
                        //We need to go in the state of war (if we're attacking)
                        if (fleet.Destination.Owner != fleet.Owner)
                      //fleet.Destination.InStateOfWar = true;
                            
                        {
                            float AttackModifier = GetActualResearchModifier("Attack");
                            float DefenseModifier = GetActualResearchModifier("Defense");
                            int AfterFight = fleet.Destination.Forces - (int)(fleet.Count * (fleet.Owner == PlayerType.Player1 ? (1 + AttackModifier) : (AIAttackBias - DefenseModifier)));
                            if (AfterFight < 0)
                            {

                                fleet.Destination.Forces = (int)(int)(fleet.Count * (fleet.Owner == PlayerType.Player1 ? (1 +AttackModifier) : (AIAttackBias - DefenseModifier))) - fleet.Destination.Forces;
                                fleet.Destination.Owner = fleet.Owner;
                                if (fleet.Destination.Captured != null)
                                {
                                    fleet.Destination.Captured();
                                }
                            }
                            else
                            {
                                fleet.Destination.Forces = AfterFight;
                            }
                        }
                        else if(fleet.Destination.Owner==fleet.Owner)
                        {
                            fleet.Destination.Forces += fleet.Count;
                        }
                             
                        
                        //if (!fleet.Destination.Forces.ContainsKey(fleet.Owner))
                        //    fleet.Destination.Forces.Add(fleet.Owner, fleet.Count);
                        //else
                        //{
                        //    fleet.Destination.Forces[fleet.Owner] += fleet.Count;
                        //}
                        
                    }
                    DeadFleets.Add(fleet);
                    fleet.Dead=true;
                }
                else
                {
                    //Enroute
                    Vector2 DirectionVector=fleet.Destination.Position - fleet.Position;
                    DirectionVector.Normalize();
                    fleet.Position +=2*DirectionVector*(1.15f+1.15f*(fleet.Owner==PlayerType.Player1?GetActualResearchModifier("Speed"):0));
                }
            }
            DeadCounter++;
            if (DeadCounter >= 60)
            {
                DeadCounter = 0;
                foreach (Fleet dead in DeadFleets)
                {
                    State.Fleets.Remove(dead);
                }
            }
        }
    }
}
