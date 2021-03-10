using System;
using System.Diagnostics;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace OrphansAdoption
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                new Harmony("mod.DisableCompanionDonations.bannerlord").PatchAll();
            }
            catch (Exception ex)
            {
                Debug.Print("Error patching:\n" + ex.Message + " \n\n" + ex.InnerException?.Message);
            }
        }
    }
}