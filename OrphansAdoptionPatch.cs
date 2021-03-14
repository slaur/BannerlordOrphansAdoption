using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace OrphansAdoption
{
    [HarmonyPatch(typeof(DestroyClanAction), "Apply")]
    internal class OrphansAdoptionPatch
    {
        private static bool Prefix(Clan destroyedClan)
        {
            var clanName = destroyedClan.Name.ToString();
            InformationManager.DisplayMessage(new InformationMessage("The " + clanName + " has been destroyed!"));

            var orphans = destroyedClan.Heroes.Where(x => x.IsChild).ToList();

            if (orphans.Count < 1) return true;

            var adoptionClan = AdoptOrphansAction.SelectFriendlyClan(destroyedClan);
            if (adoptionClan == null) return true;

            if (adoptionClan != Clan.PlayerClan)
            {
                AdoptOrphansAction.Apply(orphans, adoptionClan);
                return true;
            }
            
            // Adoption clan = Player clan : isolate temporarily the orphans to let the player decide
            foreach (var orphan in orphans) orphan.Clan = null;

            var inquiryTitleText = new TextObject("{=!}Orphans adoption");
            var inquiryText = new TextObject("{=!}The {CLAN_NAME} has been destroyed, leaving {COUNT_ORPHANS} orphans. Their leader, {LEADER_NAME}, considered you a friend : do you accept to adopt the children ?");
            inquiryText.SetTextVariable("CLAN_NAME", destroyedClan.Name);
            inquiryText.SetTextVariable("LEADER_NAME", destroyedClan.Leader.Name);
            inquiryText.SetTextVariable("COUNT_ORPHANS", orphans.Count);
            var inquiryAffirmativeText = new TextObject("{=!}Accept");
            var inquiryNegativeText = new TextObject("{=!}Refuse");
            
            InformationManager.ShowInquiry(
                new InquiryData(
                    inquiryTitleText.ToString(), inquiryText.ToString(),
                    true, true,
                    inquiryAffirmativeText.ToString(), inquiryNegativeText.ToString(),
                    () => AdoptOrphansAction.Apply(orphans, adoptionClan), 
                    () => RefuseOrphansAction.ApplyForDestroyedClan(orphans, destroyedClan)
                ), true
            );
            
            // Resume original method
            return true;
        }
    }
}