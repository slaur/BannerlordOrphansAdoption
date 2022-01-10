using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace OrphansAdoption
{
    public static class AdoptAction
    {
        private const int ChildrenLimitPerClan = 6;
        public static void ApplyByClanDestroyed(Clan destroyedClan) => ApplyForClan(destroyedClan);
        public static void ApplyByChoice(Hero child, Clan adoptionClan) => ApplyInternal(child, adoptionClan);

        private static void ApplyForClan(Clan destroyedClan)
        {
            var orphans = destroyedClan.Heroes.Where(x => x.IsChild).ToList();
            if (orphans.IsEmpty()) return;

            var friendlyClans = FriendlyClans(destroyedClan).ToList();
            if (friendlyClans.IsEmpty()) return;

            foreach (var child in orphans)
            {
                foreach (var friendlyClan in friendlyClans.Where(friendlyClan => friendlyClan.Heroes.Where(x => x.IsChild).ToList().Count < ChildrenLimitPerClan))
                {
                    ApplyInternal(child, friendlyClan);
                    break;
                }
            }
        }

        private static void ApplyInternal(Hero orphan, Clan adoptionClan)
        {
            if (adoptionClan == null) return;

            if (orphan.IsDead)
            {
                ResurrectAction.Apply(orphan);
            }

            orphan.Clan = adoptionClan;
            var updatedEncyclopediaText =
                new TextObject(
                    "{HISTORY} {?CHARACTER.GENDER}She{?}He{\\?} was adopted at the age of {CHARACTER.AGE} by the {CLAN}.");
            updatedEncyclopediaText.SetTextVariable("HISTORY", orphan.EncyclopediaText.ToString());
            StringHelpers.SetCharacterProperties("CHARACTER", orphan.CharacterObject, updatedEncyclopediaText, true);
            updatedEncyclopediaText.SetTextVariable("CLAN", adoptionClan.Name);
            orphan.EncyclopediaText = updatedEncyclopediaText;

            InformationManager.DisplayMessage(
                new InformationMessage(orphan.Name + " has been adopted by the " + adoptionClan.Name));
        }

        private static IEnumerable<Clan> FriendlyClans(Clan destroyedClan)
        {
            // Find a friendly clan in the same kingdom
            var originClanLeader = destroyedClan.Leader;
            var originKingdom = destroyedClan.Kingdom;
            if (originKingdom == null || originKingdom.IsEliminated || originClanLeader == null) return null;

            return originKingdom.Clans.Where(c =>
                    !c.IsEliminated && c.Leader.IsAlive && !c.IsUnderMercenaryService
                    && c.Leader != Hero.MainHero
                    && c.Leader.IsFriend(originClanLeader)
                    && c.Heroes.Where(x => x.IsChild).ToList().Count < ChildrenLimitPerClan
                )
                .OrderBy(clan => clan.Heroes.Where(x => x.IsChild).ToList().Count)
                .ThenBy(clan => CharacterRelationManager.GetHeroRelation(originClanLeader, clan.Leader))
                .ToList();
        }
    }
}