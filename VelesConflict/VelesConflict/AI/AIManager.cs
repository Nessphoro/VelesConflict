using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelesConflict.Gameplay;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using VelesConflict.Shared;

namespace VelesConflict.AI
{
    enum AIActionType
    {
        Attack,Deffend
    }
    class AIAction
    {
        public AIActionType ActionType { get; set; }
        public Planet Target { get; set; }
    }
    class AIManager
    {
        List<AIAction> Actions;
        TimeSpan SinceLastCommand;
        Random rnd = new Random();
        public AIManager()
        {
            Actions = new List<AIAction>();
        }

        public void ProcessTurn(GameManager gm,GameTime gt)
        {
            SinceLastCommand += gt.ElapsedGameTime;
            if (gm.State.AIPlanets.Count == 0)
                return;
            int Difficulty = (int)GameBase.playerData.Difficulty;
            if (Difficulty == 1 && SinceLastCommand.TotalSeconds < 5)
            {
                
                return;
            }

            GenerateActionPlan(gm);
            ImplementActionPlan(gm);
        }

        private void ImplementActionPlan(GameManager gm)
        {
            int Difficulty = (int)GameBase.playerData.Difficulty;
            IEnumerable<AIAction> DefenseActions = from action in Actions where action.ActionType == AIActionType.Deffend select action;
            IEnumerable<AIAction> AttackActions = from action in Actions where action.ActionType == AIActionType.Attack select action;

            //Process defense first
            foreach (AIAction action in DefenseActions)
            {
                IEnumerable<Fleet> AttackingFleets = (from fleet in gm.State.Fleets where fleet.Destination == action.Target && fleet.Owner == PlayerType.Player1 select fleet);
                IEnumerable<Fleet> DefendingFleets = (from fleet in gm.State.Fleets where fleet.Destination == action.Target && fleet.Owner == PlayerType.Player2 select fleet);

                int NetAttackForce = AttackingFleets.Sum(f => f.Count);
                NetAttackForce *= (int)((1 + GameBase.playerData.Research["Attack"] * 0.05f));
                int NetDefenseForce = DefendingFleets.Sum(f => f.Count)+action.Target.Forces;

                if (NetAttackForce > NetDefenseForce)
                {
                    if (Difficulty <=1)
                        break;
                    //Get some more troops
                    IEnumerable<Planet> supportPlanets = (from planet in gm.State.AIPlanets where planet != action.Target orderby planet.Forces ascending select planet);
                    int SupportForces=0;
                    List<Planet> sp = new List<Planet>();
                    foreach (Planet p in supportPlanets)
                    {
                        if (SupportForces + NetDefenseForce > NetAttackForce)
                            break;

                        sp.Add(p);
                        SupportForces += p.Forces / 2;
                    }
                    if (SupportForces + NetDefenseForce > NetAttackForce)
                    {
                        //We can save the planet
                        foreach (Planet planet in sp)
                        {
                            SinceLastCommand = TimeSpan.Zero;
                            gm.SendFleet(planet.Forces / 2, planet, action.Target);
                        }
                    }
                    else
                    {
                        //We can't, well shit
                    }
                }
            }

            foreach (AIAction action in AttackActions)
            {
                IEnumerable<Fleet> AttackingFleets = (from fleet in gm.State.Fleets where fleet.Destination == action.Target && fleet.Owner == PlayerType.Player2 select fleet);
                IEnumerable<Fleet> DefendingFleets = (from fleet in gm.State.Fleets where fleet.Destination == action.Target && fleet.Owner == PlayerType.Player1 select fleet);
                float MaxDistance = gm.State.AIPlanets.Max(p => Vector2.Distance(p.Position, action.Target.Position));
                int NetAttackForce = AttackingFleets.Sum(f => f.Count);
                int NetDefenseForce = DefendingFleets.Sum(f => f.Count) + action.Target.Forces;
                NetDefenseForce*=(int)((1+GameBase.playerData.Research["Defense"]*0.05f));
                if (action.Target.Owner != PlayerType.Neutral)
                {
                    NetDefenseForce+=(int)(action.Target.Growth * (MaxDistance / 1.15f / action.Target.GrowthReset));
                }
                if (NetDefenseForce > NetAttackForce)
                {
                    //Send more troops
                    IEnumerable<Planet> supportPlanets = (from planet in gm.State.AIPlanets where !gm.State.Fleets.Any(f=>f.Owner==PlayerType.Player1&&f.Destination==planet) orderby planet.Forces ascending select planet);
                    Planet firstAttackPlanet = supportPlanets.LastOrDefault(p => (p.Forces / 2 + NetAttackForce) > NetDefenseForce); //Do we have a planet that can attack by itself?
                    if (firstAttackPlanet != null)
                    {
                        SinceLastCommand = TimeSpan.Zero;
                        gm.SendFleet(firstAttackPlanet.Forces / 2, firstAttackPlanet, action.Target);
                    }
                    else if(Difficulty>0)
                    {
                        int SupportForces = 0;
                        List<Planet> sp = new List<Planet>();
                        foreach (Planet p in supportPlanets)
                        {
                            if (SupportForces + NetAttackForce > NetDefenseForce)
                                break;

                            sp.Add(p);
                            SupportForces += p.Forces / 2;
                        }
                        if (SupportForces + NetAttackForce > NetDefenseForce)
                        {
                            //We can capture the planet
                            foreach (Planet planet in sp)
                            {
                                SinceLastCommand = TimeSpan.Zero;
                                gm.SendFleet(planet.Forces / 2, planet, action.Target);
                            }
                        }
                        else
                        {
                            //We can't, well shit
                        }
                    }
                }
            }
        }

        private void GenerateActionPlan(GameManager gm)
        {
            int Difficulty=(int)GameBase.playerData.Difficulty;
            //Santize the data
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].ActionType == AIActionType.Deffend && Actions[i].Target.Owner != PlayerType.Player2)
                {
                    Actions.RemoveAt(i);
                    i--;
                }
                else if (Actions[i].ActionType == AIActionType.Attack && Actions[i].Target.Owner == PlayerType.Player2)
                {
                    Actions.RemoveAt(i);
                    i--;
                }
            }

            //Need to defend?
            foreach (Planet MyPlanet in gm.State.AIPlanets)
            {
                //Is this planet under attack already?
                bool UnderAttack = Actions.Any(action => action.Target == MyPlanet&&action.ActionType==AIActionType.Deffend);
                if (UnderAttack)
                    continue;//Nothing to do here

                IEnumerable<Fleet> AttackingFleets = (from fleet in gm.State.Fleets where fleet.Destination == MyPlanet && fleet.Owner == PlayerType.Player1 select fleet);
                if (UnderAttack && AttackingFleets.Count() == 0)
                    Actions.Remove(Actions.First(a => a.Target == MyPlanet));
                
                //Can we deal with it?
                int NetForce = AttackingFleets.Sum(f=>f.Count);

                if (NetForce > MyPlanet.Forces)
                {
                    //Defend 
                    AIAction action = new AIAction();
                    action.ActionType = AIActionType.Deffend;
                    action.Target = MyPlanet;
                    Actions.Add(action);
                }
            }

            //Lets see if we want to attack something
            Dictionary<Planet, float> MeanDistance = new Dictionary<Planet, float>();
            foreach (Planet planet in gm.State.Planets)
            {
                if (planet.Owner != PlayerType.Player2)
                {
                    float Distance = 0;
                    int Count = 0;
                    foreach (Planet myplanet in gm.State.GetMyPlanets(PlayerType.Player2))
                    {
                        Distance += Vector2.Distance(planet.Position, myplanet.Position);
                        Count++;
                    }
                    Distance /= Count;
                    MeanDistance.Add(planet, Distance);
                }
            }
            IEnumerable<Planet> AttackablePlanets = (from planet in gm.State.Planets where planet.Owner != PlayerType.Player2 orderby (10 * planet.Forces - (10f * planet.Growth*planet.GrowthReset) + 5*MeanDistance[planet] + Difficulty==1?rnd.Next(0,2000)-1000:0) ascending select planet);
            Planet first = AttackablePlanets.FirstOrDefault();
            if (first == null)
                return;
            if (Difficulty<=2)
            {
                if (!Actions.Any(a => a.Target == first))
                {
                    AIAction action = new AIAction();
                    action.ActionType = AIActionType.Attack;
                    action.Target = first;
                    Actions.Add(action);
                }
            }
            else
            {
                foreach (Planet p in AttackablePlanets)
                {
                    if (!Actions.Any(a => a.Target == p))
                    {
                        AIAction action = new AIAction();
                        action.ActionType = AIActionType.Attack;
                        action.Target = p;
                        Actions.Add(action);
                    }
                }
            }
        }
    }
}
