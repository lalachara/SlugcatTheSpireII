﻿using MegaCrit.Sts2.Core.Commands.Builders;
  using MegaCrit.Sts2.Core.Entities.Cards;
  using MegaCrit.Sts2.Core.Entities.Players;
  using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
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
public sealed class MushroomPower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/MushRoom128.png",
    BigIconPath: "res://Resource/Powers/MushRoom128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
  {
    await CreatureCmd.GainMaxHp(Owner, Amount);
    await base.AfterCardExhausted(choiceContext, card, causedByEthereal);
  }
  
  public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
  {
    Dictionary<Creature, List<DamageResult>> damageResultsByCreature;
    if (command.Attacker != Owner)
      damageResultsByCreature = (Dictionary<Creature, List<DamageResult>>) null;
    else if (command.TargetSide == Owner.Side)
      damageResultsByCreature = (Dictionary<Creature, List<DamageResult>>) null;
    else if (!command.DamageProps.IsPoweredAttack())
    {
    }
    else
    {
      List<DamageResult> list = command.Results.SelectMany<List<DamageResult>, DamageResult>((Func<List<DamageResult>, IEnumerable<DamageResult>>) (r => (IEnumerable<DamageResult>) r)).ToList<DamageResult>();
      if (!list.Any<DamageResult>((Func<DamageResult, bool>) (r => r.UnblockedDamage > 0)))
      {
      }
      else
      {
        damageResultsByCreature = new Dictionary<Creature, List<DamageResult>>();
        foreach (DamageResult damageResult in list)
        {
          if (damageResult.Receiver.IsPlayer)
          {
            if (!damageResultsByCreature.ContainsKey(damageResult.Receiver))
              damageResultsByCreature.Add(damageResult.Receiver, new List<DamageResult>());
            damageResultsByCreature[damageResult.Receiver].Add(damageResult);
          }
        }
        bool anyWoundApplied = false;
        foreach (Creature key in damageResultsByCreature.Keys)
        {
          int num = damageResultsByCreature[key].Count<DamageResult>((Func<DamageResult, bool>) (r => r.UnblockedDamage > 0));
          anyWoundApplied |= num > 0;
          await CardPileCmd.AddToCombatAndPreview<Dazed>(key, PileType.Discard, num, (Player) null);
        }
        if (anyWoundApplied)
        {
          Flash();
        }
      }
    }
  }
  
  
}