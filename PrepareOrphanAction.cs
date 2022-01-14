using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace OrphansAdoption
{
    public static class PrepareOrphanAction
    {
        public static void Apply(Hero hero) => MakeAlive(hero);

        private static void MakeAlive(Hero hero)
        {
            if (hero.IsAlive) return;
            
            if ((double) hero.BirthDay.ElapsedYearsUntilNow >= Campaign.Current.Models.AgeModel.HeroComesOfAge) return;
            var deathAge = (int) hero.Age;

            hero.ChangeState(Hero.CharacterStates.Active);
            hero.DeathDay = CampaignTime.Never;
            var currentAge = (int) hero.Age;

            var becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            var becomeTeenagerAge = Campaign.Current.Models.AgeModel.BecomeTeenagerAge;
            
            if (deathAge < becomeChildAge && currentAge > becomeChildAge)
                CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
            if (deathAge < becomeTeenagerAge && currentAge > becomeTeenagerAge)
                CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(hero);
            
            hero.EncyclopediaText = new TextObject(Hero.SetHeroEncyclopediaTextAndLinks(hero));
        }
    }
}