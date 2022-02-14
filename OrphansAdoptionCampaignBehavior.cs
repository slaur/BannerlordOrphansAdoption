using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
      campaignGameStarter.AddGameMenuOption("town", "town_orphanage_adopt", "Adopt children",
        game_menu_orphanage_adopt_on_condition, game_menu_orphanage_adopt_on_consequence, false, 9);
    }

    private static bool game_menu_orphanage_adopt_on_condition(MenuCallbackArgs args)
    {
      args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
      var canAdopt = OrphanAdoptionHelper.ChildrenForAdoptionAtSettlement(Hero.MainHero.CurrentSettlement).Any();
      // ReSharper disable once InvertIf
      if (!canAdopt)
      {
        args.Tooltip = new TextObject("There is no one here.");
        args.IsEnabled = false;
      }
      return true;
    }

    private static void game_menu_orphanage_adopt_on_consequence(MenuCallbackArgs args)
    {
      ShowChildrenToAdopt();
    }

    private static void ShowChildrenToAdopt()
    {
      var currentSettlement = Hero.MainHero.CurrentSettlement;
      var lostChildren = OrphanAdoptionHelper.ChildrenForAdoptionAtSettlement(currentSettlement);

      var inquiryElements = new List<InquiryElement>();
      foreach (var hero in lostChildren)
      {
        var identifier = new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject));
        inquiryElements.Add(new InquiryElement(hero,
          hero.Name + " - " + Math.Floor(hero.BirthDay.ElapsedYearsUntilNow),
          identifier));
      }

      if (inquiryElements.Count < 1)
      {
        InformationManager.ShowInquiry(
          new InquiryData("No child",
            "Empty orphanage", true, false, "OK", "", null, null),
          true);
        GameMenu.SwitchToMenu("town");
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
      GameMenu.SwitchToMenu("town");
    }

    private static void OnBeforeHeroKilled(Hero victim, Hero killer,
      KillCharacterAction.KillCharacterActionDetail detail,
      bool showNotification = true)
    {
      if (victim.IsHumanPlayerCharacter) return;
      if (victim.Clan == null) return;

      var clanHasMembers = victim.Clan.Heroes.Any(x =>
      {
        if (x.IsChild || x == victim || !x.IsAlive)
          return false;
        return x.IsNoble || x.IsMinorFactionHero;
      });

      var isClanDestroyed = !clanHasMembers && !victim.Clan.IsEliminated && !victim.Clan.IsBanditFaction &&
                            !victim.Clan.IsNeutralClan &&
                            (victim.Clan.Leader == victim || victim.Clan.Leader == null);

      if (isClanDestroyed)
        AdoptAction.ApplyByClanDestroyed(victim.Clan);
    }
  }
}