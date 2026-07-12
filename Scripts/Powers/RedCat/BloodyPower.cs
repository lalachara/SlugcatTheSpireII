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
public sealed class BloodyPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/BloodyPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/BloodyPower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

 // 修改前
 public override PowerStackType StackType => PowerStackType.Counter;

 private bool _triggered;

 public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props,
     Creature? dealer, CardModel? cardSource)
 {
     if (target == base.Owner && amount > 0&&props!=ValueProp.Unpowered)
     {
         _triggered = true;
         return Math.Max(0, amount - 6);
     }
     return amount;
 }

 public override async Task AfterModifyingHpLostAfterOsty()
 {
     if (_triggered)
     {
         _triggered = false;
         await PowerCmd.Decrement(this);
         await CardPileCmd.AddGeneratedCardToCombat(
             base.CombatState.CreateCard<Wound>(base.Owner.Player), PileType.Hand, base.Owner.Player);
     }
 }


}