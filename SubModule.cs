using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace OrphansAdoption
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }

            ((CampaignGameStarter)gameStarterObject).AddBehavior(new OrphansAdoptionCampaignBehavior());
        }
    }
}