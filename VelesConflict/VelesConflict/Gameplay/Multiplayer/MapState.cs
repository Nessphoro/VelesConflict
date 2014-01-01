using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Gameplay.Multiplayer
{

    public abstract class GameAction
    {
        public int SimulationTick { get; internal set; }
        public int ActionType { get; internal set; }
        public abstract GameAction CreateCopy();
    }
    public class SendRequest : GameAction
    {
        public PlayerType Owner;
        public int Origin;
        public int Destination;
        public int Ammount;

        public SendRequest()
        {
            ActionType = 1;
        }

        public override GameAction CreateCopy()
        {
            SendRequest copy = new SendRequest();
            copy.ActionType = ActionType;
            copy.SimulationTick = SimulationTick;
            copy.Owner = Owner;
            copy.Origin = Origin;
            copy.Destination = Destination;
            copy.Ammount = Ammount;

            return copy;
        }
    }


    public class MultiplayerMapState
    {
        public int StateFrame { get; private set; }
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

        public List<GameAction> Actions { get; set; }


        MultiplayerGameManager Manager;
        public MultiplayerMapState(MultiplayerGameManager manager)
        {
            Manager = manager;
            Actions = new List<GameAction>();
            Fleets = new List<Fleet>();
            Planets = new List<Planet>();
            StateFrame = 0;
        }

        public MultiplayerMapState(MultiplayerGameManager manager, int Frame)
        {
            Manager = manager;
            Actions = new List<GameAction>();
            Fleets = new List<Fleet>();
            Planets = new List<Planet>();
            StateFrame = Frame;
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

            fleet.Position = Origin.Position + Normalized;
            fleet.Origin = Origin.Id;
            fleet.Destination = Destination.Id;
            fleet.Count = Ammount;

            int ExtraShipCount = 0;
            if (fleet.Count - 10 >= 10)
                ExtraShipCount = (fleet.Count - 10) / 10;
            fleet.Positions = new Vector2[ExtraShipCount];
            Normalized.Normalize();

            for (int i = 0; i < ExtraShipCount; i++)
            {
                float Angle = MathHelper.ToRadians((float)(Manager.rnd.Next(360) + Manager.rnd.NextDouble()));
                float Distance = 8 + (float)Math.Sqrt(Manager.rnd.NextDouble()) * ((1 + (float)Math.Sqrt(ExtraShipCount) / 2) * 8);
                fleet.Positions[i] = new Vector2((float)Math.Cos(Angle) * Distance, (float)Math.Sin(Angle) * Distance);

                foreach (Vector2 p in fleet.Positions)
                {
                    if (p == null || p == fleet.Positions[i])
                        continue;

                    if (Vector2.Distance(p, fleet.Positions[i]) < 16)
                    {
                        Angle = MathHelper.ToRadians((float)(Manager.rnd.Next(360) + Manager.rnd.NextDouble()));
                        Distance = 8 + (float)Math.Sqrt(Manager.rnd.NextDouble()) * ((1 + (float)Math.Sqrt(ExtraShipCount) / 2) * 8);
                        fleet.Positions[i] = new Vector2((float)Math.Cos(Angle) * Distance, (float)Math.Sin(Angle) * Distance);
                    }
                }
            }

            Fleets.Add(fleet);
        }

        public void SendFleet(int Ammount, int OriginID, int DestinationID)
        {
            Planet Origin = null, Destination = null;
            foreach (Planet planet in Planets)
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

        public void Update(int Frames)
        {
            StateFrame -= Frames;
            Actions = Actions.OrderBy(g1 => g1.SimulationTick).ToList();
            for (int f = 0; f < Frames; f++)
            {
                #region Do Work

                for (int i = 0; i < Actions.Count; i++)
                {
                    GameAction gA = Actions[i];
                    if (gA.SimulationTick <= StateFrame)
                    {
                        switch (gA.ActionType)
                        {
                            case 1:
                                SendRequest sr = (SendRequest)gA;
                                Planet Origin = FromId(sr.Origin);
                                if (Origin.Owner == sr.Owner)
                                {
                                    if (sr.Ammount == 0)
                                        sr.Ammount = Origin.Forces / 2;

                                    SendFleet(sr.Ammount, sr.Origin, sr.Destination);
                                }

                                break;
                        }
                        Actions.RemoveAt(i);
                        i--;
                    }

                }


                foreach (Planet planet in Planets)
                {

                    //Process growth
                    if (planet.Owner != PlayerType.Neutral)
                    {
                        planet.GrowthCounter--;
                        if (planet.GrowthCounter <= 0)
                        {
                            planet.Forces += planet.Growth;
                            planet.GrowthCounter = planet.GrowthReset;
                        }
                    }





                }

                List<Fleet> DeadFleets = new List<Fleet>();
                foreach (Fleet fleet in Fleets)
                {
                    Planet Destination = (from p in Planets where fleet.Destination == p.Id select p).First();
                    Planet Origin = (from p in Planets where fleet.Origin == p.Id select p).First();
                    if (Vector2.Distance(fleet.Position, Destination.Position) < Destination.PlanetSize * 64)
                    {
                        //We have arrived at a planet
                        if (Destination.Owner == PlayerType.Neutral && Destination.Forces < fleet.Count)
                        {
                            //If the owner is neutral capture/attack is instantanious; otherwise process in the next turn
                            Destination.Owner = fleet.Owner;
                            Destination.Forces = fleet.Count - Destination.Forces;
                            if (Destination.Captured != null)
                            {
                                Destination.Captured();
                            }
                        }
                        else if (Destination.Owner == PlayerType.Neutral)
                        {
                            Destination.Forces -= fleet.Count;
                            if (Destination.Forces == 0)
                            {
                                Destination.Owner = fleet.Owner;
                            }
                        }
                        else
                        {
                            //We need to go in the state of war (if we're attacking)
                            if (Destination.Owner != fleet.Owner)
                            //fleet.Destination.InStateOfWar = true;
                            {
                                int AfterFight = Destination.Forces - fleet.Count;
                                if (AfterFight < 0)
                                {

                                    Destination.Forces = fleet.Count-Destination.Forces;
                                    Destination.Owner = fleet.Owner;
                                    if (Destination.Captured != null)
                                    {
                                        Destination.Captured();
                                    }
                                }
                                else
                                {
                                    Destination.Forces = AfterFight;
                                }
                            }
                            else if (Destination.Owner == fleet.Owner)
                            {
                                Destination.Forces += fleet.Count;
                            }


                            //if (!fleet.Destination.Forces.ContainsKey(fleet.Owner))
                            //    fleet.Destination.Forces.Add(fleet.Owner, fleet.Count);
                            //else
                            //{
                            //    fleet.Destination.Forces[fleet.Owner] += fleet.Count;
                            //}

                        }
                        DeadFleets.Add(fleet);
                    }
                    else
                    {
                        //Enroute
                        Vector2 DirectionVector = Destination.Position - fleet.Position;
                        DirectionVector.Normalize();
                        fleet.Position += DirectionVector * (1.15f);
                    }
                }
                foreach (Fleet dead in DeadFleets)
                {
                    Fleets.Remove(dead);
                }
                #endregion
                StateFrame++;
            }
        }
        public Planet FromId(int Id)
        {
            return (from p in Planets where p.Id == Id select p).First();
        }
        public void Update()
        {
            if (StateFrame < 0)
            {
                StateFrame++;
                return;
            }

            //Actions = Actions.OrderBy(g1 => g1.SimulationTick).ToList();
            for (int i = 0; i < Actions.Count; i++)
            {
                GameAction gA = Actions[i];
                if (gA.SimulationTick <= StateFrame)
                {
                    switch (gA.ActionType)
                    {
                        case 1:
                            SendRequest sr = (SendRequest)gA;
                            Planet Origin = FromId(sr.Origin);
                            if (Origin.Owner == sr.Owner)
                            {
                                if (sr.Ammount == 0)
                                    sr.Ammount = Origin.Forces / 2;

                                SendFleet(sr.Ammount, sr.Origin, sr.Destination);
                            }

                            break;
                    }
                    Actions.RemoveAt(i);
                    i--;
                }

            }


            foreach (Planet planet in Planets)
            {

                //Process growth
                if (planet.Owner != PlayerType.Neutral)
                {
                    planet.GrowthCounter--;
                    if (planet.GrowthCounter <= 0)
                    {
                        planet.Forces += planet.Growth;
                        planet.GrowthCounter = planet.GrowthReset;
                    }
                }





            }

            List<Fleet> DeadFleets = new List<Fleet>();
            foreach (Fleet fleet in Fleets)
            {
                Planet Destination = (from p in Planets where fleet.Destination == p.Id select p).First();
                Planet Origin = (from p in Planets where fleet.Origin == p.Id select p).First();
                if (Vector2.Distance(fleet.Position, Destination.Position) < Destination.PlanetSize * 64)
                {
                    //We have arrived at a planet
                    if (Destination.Owner == PlayerType.Neutral && Destination.Forces < fleet.Count)
                    {
                        Destination.Owner = fleet.Owner;
                        Destination.Forces = fleet.Count - Destination.Forces;
                        if (Destination.Captured != null)
                        {
                            Destination.Captured();
                        }
                    }
                    else if (Destination.Owner == PlayerType.Neutral)
                    {
                        Destination.Forces -= fleet.Count;
                        if (Destination.Forces == 0)
                        {
                            Destination.Owner = fleet.Owner;
                        }
                    }
                    else
                    {
                        //We need to go in the state of war (if we're attacking)
                        if (Destination.Owner != fleet.Owner)
                        //fleet.Destination.InStateOfWar = true;
                        {
                            int AfterFight = Destination.Forces - fleet.Count;
                            if (AfterFight < 0)
                            {

                                Destination.Forces = fleet.Count-Destination.Forces;
                                Destination.Owner = fleet.Owner;
                                if (Destination.Captured != null)
                                {
                                    Destination.Captured();
                                }
                            }
                            else
                            {
                                Destination.Forces = AfterFight;
                            }
                        }
                        else if (Destination.Owner == fleet.Owner)
                        {
                            Destination.Forces += fleet.Count;
                        }

                    }
                    DeadFleets.Add(fleet);
                }
                else
                {
                    //Enroute
                    Vector2 DirectionVector = Destination.Position - fleet.Position;
                    DirectionVector.Normalize();
                    fleet.Position += DirectionVector * (1.15f );
                }
            }
            foreach (Fleet dead in DeadFleets)
            {
                Fleets.Remove(dead);
            }

            StateFrame++;
        }
    }
}
