using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace OrphansAdoption
{
  public static class OrphanAdoptionHelper
  {
    public static IEnumerable<Hero> ChildrenForAdoptionAtSettlement(Settlement settlement)
    {
      return Campaign.Current.DeadOrDisabledHeroes.Where(hero =>
        hero.IsChild
        && hero.IsNoble
        && (hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.Lost ||
            hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.None)
        && hero.BornSettlement == settlement
        && (double) hero.BirthDay.ElapsedYearsUntilNow < Campaign.Current.Models.AgeModel.HeroComesOfAge
      ).ToList();
    }
  }
}