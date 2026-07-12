﻿using Godot;
 using HarmonyLib;
 using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.relics;
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
public sealed class ChuangPower : ModPowerTemplate
{
  
  public override PowerType Type => PowerType.Debuff;
  private Boolean active = false;
  
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/ChuangImg128.png",
    BigIconPath: "res://Resource/Powers/ChuangImg128.png"
  );

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
  {
    if (command.Attacker == base.Owner) 
    {
      
      int hitCount = Traverse.Create(command).Field("_hitCount").GetValue<int>();
      for(int i=0;i<hitCount;i++)
      {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount,  ValueProp.Unpowered, null, null);
      }
      active = true;
      if (getfireblood() && Owner.Side!=CombatSide.Player)
      {
        await PowerCmd.ModifyAmount(choiceContext,this, 1, Owner,null);
      }
      
    }
    await base.AfterAttack(choiceContext, command);
  }

  // public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
  //   Creature target, CardModel? cardSource)
  // {
  //   if (dealer != null && (dealer == base.Owner || dealer.PetOwner?.Creature == base.Owner) )
  //   {
  //     await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount,  ValueProp.Unpowered, null, null);
  //     active = true;
  //     if (getfireblood() && Owner.Side!=CombatSide.Player)
  //     {
  //       await PowerCmd.ModifyAmount(choiceContext,this, 1, Owner,null);
  //     }
  //     
  //   }
  //
  // }

  public override async Task BeforeSideTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IEnumerable<Creature> participants)	{
    if (side == Owner.Side)
    {
      if (!active&&!getfireblood())
      {
        await PowerCmd.Decrement(this);
      }
      else
      {
        active = false;
      }
    }
  }

  public bool getfireblood()
  {
    foreach (Player player in CombatState.Players)
    {
      if (player.GetRelic<Liver_Fireblood>() != null)
      {
        player.GetRelic<Liver_Fireblood>().Flash();
        return true;
      }
    } 
    return false;
  }


}