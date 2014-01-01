using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Gameplay.Multiplayer
{
    public class MultiplayerGameManager
    {
        public MultiplayerMapState LeadingState
        {
            get;
            set;
        }
        private MultiplayerMapState TrailingState0 { get; set; }
        private MultiplayerMapState TrailingState1 { get; set; }
        private MultiplayerMapState TrailingState2 { get; set; }
        private MultiplayerMapState TrailingState3 { get; set; }
        private MultiplayerMapState TrailingState4 { get; set; }
        internal Random rnd=new Random();

        public MultiplayerGameManager()
        {
            LeadingState = new MultiplayerMapState(this);
            TrailingState0 = new MultiplayerMapState(this, -15);
            TrailingState1 = new MultiplayerMapState(this, -30);
            TrailingState2 = new MultiplayerMapState(this, -45);
            TrailingState3 = new MultiplayerMapState(this, -60);
            TrailingState4 = new MultiplayerMapState(this, -120);
        }

        public void AddPlanet(Planet planet)
        {
            LeadingState.Planets.Add(planet);
            TrailingState0.Planets.Add(new Planet(planet));
            TrailingState1.Planets.Add(new Planet(planet));
            TrailingState2.Planets.Add(new Planet(planet));
            TrailingState3.Planets.Add(new Planet(planet));
            TrailingState4.Planets.Add(new Planet(planet));
        }

        public void SendFleet(int Ammount, Planet Origin, Planet Destination,int Tick)
        {
            SendFleet(Ammount, Origin.Id, Destination.Id,Tick);
        }
        public void SendFleet(int Ammount, int OriginID, int DestinationID,int Tick)
        {
            SendRequest request = new SendRequest();
            request.Ammount = Ammount;
            request.Destination = DestinationID;
            request.Origin = OriginID;
            request.Owner = LeadingState.FromId(OriginID).Owner;
            request.SimulationTick = Tick;
            QueueAction(request);
        }

        public void QueueAction(GameAction action)
        {
            LeadingState.Actions.Add(action);
            TrailingState0.Actions.Add(action.CreateCopy());
            TrailingState1.Actions.Add(action.CreateCopy());
            TrailingState2.Actions.Add(action.CreateCopy());
            TrailingState3.Actions.Add(action.CreateCopy());
            TrailingState4.Actions.Add(action.CreateCopy());
        }

        public bool GameEnd()
        {
            bool ret = false;
            PlayerType looser=PlayerType.Neutral;
            if (LeadingState.GetMyPlanets(PlayerType.Player1).Count == 0)
            {
                looser = PlayerType.Player1;
                ret = true;
            }
            if (LeadingState.GetMyPlanets(PlayerType.Player2).Count == 0)
            {
                looser = PlayerType.Player2;
                ret = true;
            }
            if (looser != PlayerType.Neutral&&ret==true)
            {
                foreach (Fleet f in LeadingState.Fleets)
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
            if (LeadingState.GetMyPlanets(PlayerType.Player1).Count == 0)
            {
                looser = PlayerType.Player1;
                ret = true;
            }
            if (LeadingState.GetMyPlanets(PlayerType.Player2).Count == 0)
            {
                looser = PlayerType.Player2;
                ret = true;
            }
            if (looser != PlayerType.Neutral && ret == true)
            {
                foreach (Fleet f in LeadingState.Fleets)
                {
                    if (f.Owner == looser)
                        ret = false;
                }
            }

            return looser;
        }
        

        public void Update()
        {
            CorrectInconsistencies();

            LeadingState.Update();
            TrailingState0.Update();
            TrailingState1.Update();
            TrailingState2.Update();
            TrailingState3.Update();
            TrailingState4.Update();
        }

        void Rollback(MultiplayerMapState Source, MultiplayerMapState Destination)
        {
            Destination.Actions.Clear();
            Destination.Planets.Clear();
            Destination.Fleets.Clear();
            foreach (Planet planet in Source.Planets)
            {
                Destination.Planets.Add(new Planet(planet));
            }
            foreach (Fleet fleet in Source.Fleets)
            {
                Destination.Fleets.Add(new Fleet(fleet));
            }
            Destination.Actions.AddRange((from ga in Source.Actions select ga.CreateCopy()));
            Destination.Update(Destination.StateFrame - Source.StateFrame);
        }

        private void CorrectInconsistencies()
        {
            //Detect inconsistencies
            List<GameAction> OutOfOrder = (from action in TrailingState3.Actions where (action.SimulationTick < TrailingState3.StateFrame && (TrailingState3.StateFrame - action.SimulationTick) < 60) select action).ToList();
            if (OutOfOrder.Count > 0)
            {
                Rollback(TrailingState4, TrailingState3);
                Rollback(TrailingState3, TrailingState2);
                Rollback(TrailingState2, TrailingState1);
                Rollback(TrailingState1, TrailingState0);
                Rollback(TrailingState0, LeadingState);
                return;
            }

            OutOfOrder = (from action in TrailingState2.Actions where (action.SimulationTick < TrailingState2.StateFrame && (TrailingState2.StateFrame - action.SimulationTick) < 15) select action).ToList();
            if (OutOfOrder.Count > 0)
            {
                Rollback(TrailingState3, TrailingState2);
                Rollback(TrailingState2, TrailingState1);
                Rollback(TrailingState1, TrailingState0);
                Rollback(TrailingState0, LeadingState);
                return;
            }

            OutOfOrder = (from action in TrailingState1.Actions where (action.SimulationTick < TrailingState1.StateFrame && (TrailingState1.StateFrame - action.SimulationTick) < 15) select action).ToList();
            if (OutOfOrder.Count > 0)
            {
                Rollback(TrailingState2, TrailingState1);
                Rollback(TrailingState1, TrailingState0);
                Rollback(TrailingState0, LeadingState);
                return;
            }

            OutOfOrder = (from action in TrailingState0.Actions where (action.SimulationTick < TrailingState0.StateFrame && (TrailingState0.StateFrame - action.SimulationTick) < 15) select action).ToList();
            if (OutOfOrder.Count > 0)
            {
                Rollback(TrailingState1, TrailingState0);
                Rollback(TrailingState0, LeadingState);
                return;
            }

            OutOfOrder = (from action in LeadingState.Actions where (action.SimulationTick < LeadingState.StateFrame && (LeadingState.StateFrame - action.SimulationTick) < 15) select action).ToList();
            if (OutOfOrder.Count > 0)
            {
                Rollback(TrailingState0, LeadingState);
                return;
            }
            //
        }
    }
}
