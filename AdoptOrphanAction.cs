using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace OrphansAdoption
{
    public static class AdoptOrphanAction
    {
        // todo add log entry for each child, add log entry for clan
        private static void ApplyInternal(Hero orphan)
        {
            var adoptionClan = SelectAdoptionClanForHero(orphan);
            if (adoptionClan == null) return;
            orphan.Clan = adoptionClan;
            InformationManager.DisplayMessage(
                new InformationMessage(orphan.Name + " has been adopted by " + adoptionClan.Name));
        }

        public static void Apply(Hero orphan) => ApplyInternal(orphan);

        private static Clan SelectAdoptionClanForHero(Hero hero)
        {
            if (!hero.IsChild) return null;

            // Find friendly clan in kingdom
            var originClanLeader = hero.Clan?.Leader;
            var originKingdom = hero.Clan?.Kingdom;
            if (originKingdom == null || originClanLeader == null) return null;

            var kingdomFriendlyClans = originKingdom.Clans.Where(t =>
                !t.IsMinorFaction && t.Leader.IsAlive && t.Leader.IsFriend(originClanLeader)
            ).ToList();

            if (!kingdomFriendlyClans.IsEmpty())
            {
                return kingdomFriendlyClans.OrderByDescending(clan =>
                    CharacterRelationManager.GetHeroRelation(originClanLeader, clan.Leader)
                ).First();
            }

            return null;
        }
    }
}