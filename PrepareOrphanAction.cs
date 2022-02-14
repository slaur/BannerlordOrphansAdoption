using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace OrphansAdoption
{
  public static class PrepareOrphanAction
  {
    public static void Apply(Hero hero) => MakeAlive(hero);

    // Revert TaleWorlds.CampaignSystem.Actions.KillCharacterAction.MakeDead
    private static void MakeAlive(Hero hero)
    {
      if (hero.IsAlive) return;

      var ageAtDeath = (int) hero.Age;

      hero.ChangeState(Hero.CharacterStates.Active);
      hero.DeathDay = CampaignTime.Never;
      var currentAge = (int) hero.Age;

      if (currentAge >= Campaign.Current.Models.AgeModel.HeroComesOfAge) return;

      var becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
      var becomeTeenagerAge = Campaign.Current.Models.AgeModel.BecomeTeenagerAge;
      var becomeAdult = Campaign.Current.Models.AgeModel.HeroComesOfAge;

      if (ageAtDeath < becomeChildAge && currentAge > becomeChildAge)
        CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
      if (ageAtDeath < becomeTeenagerAge && currentAge > becomeTeenagerAge)
        CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(hero);
      if (ageAtDeath < becomeAdult && currentAge > becomeAdult)
        CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);

      hero.EncyclopediaText = new TextObject(Hero.SetHeroEncyclopediaTextAndLinks(hero));
    }
  }
}