﻿using MegaCrit.Sts2.Core.Entities.Cards;
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
public sealed class FreespearPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/FreeSpear128.png",
    BigIconPath: "res://Resource/Powers/FreeSpear128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  
  public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
  {
    modifiedCost = originalCost;
    if (card.Owner.Creature != base.Owner)
    {
      return false;
    }
    if (!(card.Keywords.Contains(RainworldKeywords.Spear)))
    {
      return false;
    }
    bool flag;
    switch (card.Pile?.Type)
    {
      case PileType.Hand:
      case PileType.Play:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (!flag)
    {
      return false;
    }
    modifiedCost = default(decimal);
    return true;
  }
  
  public override async Task BeforeCardPlayed(CardPlay cardPlay)
  {
    if (cardPlay.Card.Owner.Creature == base.Owner)
    {
      bool flag;
      switch (cardPlay.Card.Pile?.Type)
      {
        case PileType.Hand:
        case PileType.Play:
          flag = true;
          break;
        default:
          flag = false;
          break;
      }
      if (flag)
      {
        await PowerCmd.Decrement(this);
      }
    }
  }

  public override async Task BeforeSideTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IEnumerable<Creature> participants)	{
    if (side == Owner.Side)
      await PowerCmd.Remove(this);
  }
}