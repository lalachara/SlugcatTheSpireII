﻿using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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
public sealed class TubePower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Tube128.png",
    BigIconPath: "res://Resource/Powers/Tube128.png"
  );
  public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public int rounder = 1;

  public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
  {
     if (side == base.Owner.Side)
        {
          Flash();
          await CreatureCmd.GainBlock(base.Owner,  base.Amount, ValueProp.Unpowered, null);
          rounder--;
          if (rounder <= 0)
          {
            await PowerCmd.Remove(this);
            Flash();
          }
        }
    await base.AfterSideTurnStartLate(side, participants, combatState);
  }

  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? _, CardModel? __)
  {
    if (target == base.Owner && props!=ValueProp.Unpowered && result.UnblockedDamage > 0)
    {
      await PowerCmd.Remove(this);
      Flash();
    }
  }
  
}