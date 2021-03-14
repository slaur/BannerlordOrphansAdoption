using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace OrphansAdoption
{
    public static class RefuseOrphansAction
    {
        private static void ApplyInternal(IEnumerable<Hero> orphans, Clan originClan)
        {
            foreach (var orphan in orphans)
            {
                // Orphan goes back to its original clan
                if (orphan.Clan == null) orphan.Clan = originClan;
                // and is ultimately removed
                KillCharacterAction.ApplyByRemove(orphan);
            }
        }

        public static void ApplyForDestroyedClan(IEnumerable<Hero> orphans, Clan destroyedClan) => ApplyInternal(orphans, destroyedClan);
    }
}