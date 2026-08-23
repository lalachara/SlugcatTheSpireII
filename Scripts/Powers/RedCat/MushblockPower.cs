﻿using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
public sealed class MushblockPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/MushBlockPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/MushBlockPower128.png"
  );
  
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  private decimal needremove = 0;
  public override decimal ModifyHpLostAfterOsty(
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
  {
    if (target != Owner || amount <= 0)
      return amount;

    if (amount >= Amount)
    {
      var remaining = amount - Amount;
      needremove = Amount;
      return remaining;
    }
    needremove = amount;
    return 0;
  }

  public override async Task AfterModifyingHpLostAfterOsty()
  {
    if(needremove==0)
      return;
    if (Amount == needremove)
    {
      await PowerCmd.Remove(this);
      return;
    }
    if (needremove > 0)
    {
      await PowerCmd.ModifyAmount(new BlockingPlayerChoiceContext(), this, -needremove, Owner, null);
      needremove = 0;
    }
    await base.AfterModifyingHpLostAfterOsty();
  }
}