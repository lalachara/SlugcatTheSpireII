﻿using Godot;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
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
public sealed class LastPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/Last128.png",
    BigIconPath: "res://Resource/Powers/RedCat/Last128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;

  public override decimal ModifyHpLostAfterOsty(
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
  {
    if (target != Owner)
      return amount;
    
    if(Owner.Player.Creature.HasPower<WorklockPower>()
       ||Owner.Player.Relics.OfType<LizardTail>().Any(r => !r.WasUsed)
       ||Owner.Player.Potions.OfType<FairyInABottle>().Any())
      return amount;
    if(SlugcatData.GetWorkLevel(Owner.Player)>0&&(!Owner.HasPower<RedmushroomPower>()||Owner.HasPower<RefusePower>()))
      return amount;
    var otherPlayersAlive = CombatState.Players.Any(p => p != Owner.Player && p.Creature.IsAlive && (!p.Creature.HasPower<FakedeathPower>()));

    if (amount >= target.CurrentHp&&otherPlayersAlive)
    {
      SlugcatData.setworklevel(Owner.Player,0);
      PowerCmd.Apply<FakedeathPower>(new BlockingPlayerChoiceContext(),Owner,1,Owner,null);
      Flash();
      if(CombatState.CurrentSide == CombatSide.Player)
        PlayerCmd.EndTurn(Owner.Player,true);
      return target.CurrentHp - 1;
    }
    return amount;
  }

}