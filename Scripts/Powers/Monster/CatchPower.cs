﻿﻿using MegaCrit.Sts2.Core.Commands.Builders;
  using MegaCrit.Sts2.Core.Entities.Cards;
  using MegaCrit.Sts2.Core.Entities.Players;
  using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Monster;
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
public sealed class CatchPower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/ThreeChicken128.png",
    BigIconPath: "res://Resource/Powers/ThreeChicken128.png"
  );

  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  bool WasJustAppliedByEnemy = true;
  
  public override async Task AfterDamageGiven(
    PlayerChoiceContext choiceContext,
    Creature? dealer,
    DamageResult result,
    ValueProp props,
    Creature target,
    CardModel? cardSource)
  {
    if (dealer != Owner || !target.IsPlayer || !props.IsPoweredAttack() || result.UnblockedDamage <= 0)
      return;
    
    await CreatureCmd.LoseMaxHp(choiceContext, target, 5, false);
  }
  
  public override async Task AfterSideTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IEnumerable<Creature> participants)
  {
    if (!participants.Contains<Creature>(Owner))
      return;
    if (WasJustAppliedByEnemy)
    {
      WasJustAppliedByEnemy = false;
    }
    else
    {
      Flash();
      StrengthPower strengthPower = await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, (Decimal) Amount, Owner, (CardModel) null);
    }
  }
  
}