﻿using MegaCrit.Sts2.Core.Entities.Cards;
 using MegaCrit.Sts2.Core.Entities.Players;
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
public sealed class HunterinstinctPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/HunterinstinctPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/HunterinstinctPower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  public int storedAmount = 0;

  // public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
  // {
  //   var enemies = base.CombatState.HittableEnemies;
  //   if (enemies.Count > 0&&Owner.Player!=null)
  //   {
  //     var target = enemies[Owner.Player.RunState.Rng.CombatTargets.NextInt(0, enemies.Count)];
  //     await PowerCmd.Apply<HuntsignPower>(choiceContext, target, 1, base.Owner, null);
  //   }
  //   await base.AfterPlayerTurnStartEarly(choiceContext, player);
  // }

  public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
  {
    if (storedAmount != 0&&cardPlay.Card.Owner==Owner.Player)
    {
      await PowerCmd.Apply<VigorPower>(choiceContext, Owner, storedAmount, base.Owner, null);
      storedAmount = 0;
    }
    await base.AfterCardPlayedLate(choiceContext, cardPlay);
  }


  public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
    Creature target, CardModel? cardSource)
  {
    if(dealer == Owner&&target.HasPower<HuntsignPower>())
      storedAmount+=Amount;
    await base.AfterDamageGiven(choiceContext, dealer, result, props, target, cardSource);
  }
}