using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace OrphansAdoption
{
    public static class AdoptOrphansAction
    {
        private static void ApplyInternal(IEnumerable<Hero> orphans, Clan adoptionClan)
        {
            if (adoptionClan == null) return;

            foreach (var orphan in orphans)
            {
                orphan.Clan = adoptionClan;
                System.Diagnostics.Debug.WriteLine(Hero.SetHeroEncyclopediaTextAndLinks(orphan));
                InformationManager.DisplayMessage(
                    new InformationMessage(orphan.Name + " has been adopted by the " + adoptionClan.Name));
            }
        }

        public static void Apply(IEnumerable<Hero> orphans, Clan adoptionClan) => ApplyInternal(orphans, adoptionClan);

        public static Clan SelectFriendlyClan(Clan destroyedClan)
        {
            // Find friendly clan in kingdom
            var originClanLeader = destroyedClan.Leader;
            var originKingdom = destroyedClan.Kingdom;
            if (originKingdom == null || originClanLeader == null) return null;
            if (originKingdom.IsEliminated) return null;

            var kingdomFriendlyClans = originKingdom.Clans.Where(c =>
                !c.IsEliminated && c.Leader.IsAlive && !c.IsUnderMercenaryService && c.Leader.IsFriend(originClanLeader)
            ).OrderByDescending(clan =>
                CharacterRelationManager.GetHeroRelation(originClanLeader, clan.Leader)
            ).ToList();

            return !kingdomFriendlyClans.IsEmpty() ? kingdomFriendlyClans.First() : null;
        }
    }
}