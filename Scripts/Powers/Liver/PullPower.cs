﻿using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Events;
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
public sealed class PullPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Pull128.png",
    BigIconPath: "res://Resource/Powers/Pull128.png"
  );
  
  public override PowerType Type => PowerType.Debuff;

  public override PowerStackType StackType => PowerStackType.Counter;
  public int basenum;

  public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
  {
      basenum = this.Amount;
      await base.AfterApplied(applier, cardSource);
  }

  // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 
 public override Decimal ModifyHpLostAfterOstyLate(
     Creature target,
     Decimal amount,
     ValueProp props,
     Creature? dealer,
     CardModel? cardSource)
 {
     if(target == this.Owner &&props!=ValueProp.Unpowered&&amount>=25)
         return amount*2; 
     return amount;
 }


 public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
 {
     if (target == base.Owner && dealer != null && props!=ValueProp.Unpowered&&_.TotalDamage>=25)
     {
         
         await PowerCmd.Decrement(this);
         if(dealer.Player != null)
            await PlayerCmd.GainEnergy(1, dealer.Player);
     }
 }
}