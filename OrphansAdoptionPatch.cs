using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace OrphansAdoption
{
    [HarmonyPatch(typeof(DestroyClanAction), "Apply")]
    internal class OrphansAdoptionPatch
    {
        private static bool Prefix(IFaction destroyedClan)
        {
            var clanName = destroyedClan.Name.ToString();
            InformationManager.DisplayMessage(new InformationMessage("The " + clanName + " was destroyed!"));

            var orphans = destroyedClan.Heroes.Where(x => x.IsChild).ToList();
            foreach (var orphan in orphans)
            {
                AdoptOrphanAction.Apply(orphan);
            }

            return true;
        }
    }
}