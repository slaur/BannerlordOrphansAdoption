using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace OrphansAdoption
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly List<Action> ActionsToExecuteNextTick = new List<Action>();
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }

            ((CampaignGameStarter)gameStarterObject).AddBehavior(new OrphansAdoptionCampaignBehavior());
        }
        
        public static void ExecuteActionOnNextTick(Action action)
        {
            if (action == null)
            {
                return;
            }

            ActionsToExecuteNextTick.Add(action);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            foreach (var action in ActionsToExecuteNextTick)
            {
                action();
            }

            ActionsToExecuteNextTick.Clear();
        }
    }
}