using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace OrphansAdoption
{
    internal class OrphansAdoptionCampaignBehavior : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.BeforeHeroKilledEvent.AddNonSerializedListener(this, OnBeforeHeroKilled);
        }

        // Menus : see TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns.PlayerTownVisitCampaignBehavior
        private static void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "town_orphanage", "Go to the orphanage",
                game_menu_town_go_to_orphanage_on_condition, x => GameMenu.SwitchToMenu("town_orphanage"), false, 1);
            campaignGameStarter.AddGameMenu("town_orphanage", "This is the orphanage", town_orphanage_on_init,
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameStarter.AddGameMenuOption("town_orphanage", "town_orphanage_adopt", "Adopt children",
                game_menu_orphanage_adopt_on_condition, game_menu_orphanage_adopt_on_consequence);
            campaignGameStarter.AddGameMenuOption("town_orphanage", "town_orphanage_back", "{=3sRdGQou}Leave",
                back_on_condition, x => GameMenu.SwitchToMenu("town"), true);
        }

        [GameMenuInitializationHandler("town_orphanage")]
        public static void game_menu_town_orphanage_on_init(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            CultureCode[] meshCulturesCodes =
            {
                CultureCode.Battania,
                CultureCode.Empire,
                CultureCode.Sturgia,
                CultureCode.Vlandia
            };
            var meshName = Array.Exists(meshCulturesCodes, t => t == settlement.Culture.GetCultureCode())
                ? settlement.Culture.GetCultureCode().ToString().ToLower()
                : "default";
            args.MenuContext.SetBackgroundMeshName(meshName + "_orphanage");
        }

        private static bool game_menu_town_go_to_orphanage_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private static void town_orphanage_on_init(MenuCallbackArgs args)
        {
        }

        private static bool game_menu_orphanage_adopt_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            return true;
        }

        private static void game_menu_orphanage_adopt_on_consequence(MenuCallbackArgs args)
        {
            ShowChildrenToAdopt();
        }

        private static bool back_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private static void ShowChildrenToAdopt()
        {
            var currentSettlement = Hero.MainHero.CurrentSettlement;
            var lostChildren = Campaign.Current.DeadOrDisabledHeroes.Where(hero =>
                hero.IsChild
                && hero.IsNoble
                && (hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.Lost ||
                    hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.None)
                && hero.BornSettlement == currentSettlement
                && (double) hero.BirthDay.ElapsedYearsUntilNow < Campaign.Current.Models.AgeModel.HeroComesOfAge
            ).ToList();

            var inquiryElements = new List<InquiryElement>();
            foreach (var hero in lostChildren)
            {
                var identifier = new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject));
                inquiryElements.Add(new InquiryElement(hero,
                    hero.Name + " - " + hero.BirthDay.ElapsedYearsUntilNow.ToString("0"),
                    identifier));
            }

            if (inquiryElements.Count < 1)
            {
                InformationManager.ShowInquiry(
                    new InquiryData("No child",
                        "Empty orphanage", true, false, "OK", "", null, null),
                    true);
                GameMenu.SwitchToMenu("town_orphanage");
            }
            else
            {
                InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    "Children you may adopt", "", inquiryElements, true, 1, "Continue", null, args =>
                    {
                        if (args == null) return;
                        if (!args.Any()) return;
                        InformationManager.HideInquiry();
                        ConfirmAdoption(args.Select(element => element.Identifier as Hero).First());
                    }, null));
            }
        }

        private static void ConfirmAdoption(Hero child)
        {
            AdoptAction.ApplyByChoice(child, Clan.PlayerClan);
            GameMenu.SwitchToMenu("town_orphanage");
        }

        private static void OnBeforeHeroKilled(Hero victim, Hero killer,
            KillCharacterAction.KillCharacterActionDetail detail,
            bool showNotification = true)
        {
            if (victim.IsHumanPlayerCharacter) return;
            var isClanDestroyed = victim.Clan != null && !victim.Clan.IsEliminated && !victim.Clan.IsBanditFaction &&
                                  !victim.Clan.IsNeutralClan &&
                                  (victim.Clan.Leader == victim || victim.Clan.Leader == null);
            if (!isClanDestroyed) return;

            AdoptAction.ApplyByClanDestroyed(victim.Clan);
        }
    }
}