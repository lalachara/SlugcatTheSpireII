﻿using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class KittyPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Kitty128.png",
    BigIconPath: "res://Resource/Powers/Kitty128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  private bool enable = true;
  public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
  {
    if (player == Owner.Player &&  Owner.Player?.Character is Slugcat slugcat)
    {
      if (SlugcatData.GetFood(Owner.Player) < 1)
      {
        enable = false;
        return;
      }
      await SlugcatData.addfood(Owner.Player,-1);
      enable = true;
      await CardPileCmd.Draw(choiceContext, base.Amount, Owner.Player);
          await PlayerCmd.GainEnergy(base.Amount, Owner.Player);
    }


  }
  
  public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
  {
    if (player != base.Owner.Player || !Hook.ShouldFlush(player.Creature.CombatState, player) || !enable)
    {
      return;
    }
    List<CardModel> list = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.Amount), context: choiceContext, player: base.Owner.Player, filter: RetainFilter, source: this)).ToList();
    if (list.Count == 0)
    {
      return;
    }
    foreach (CardModel item in list)
    {
      item.GiveSingleTurnRetain();
    }
  }

  private bool RetainFilter(CardModel card)
  {
    return !card.ShouldRetainThisTurn;
  }
  
}