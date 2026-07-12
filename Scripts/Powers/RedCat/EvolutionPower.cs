﻿﻿using MegaCrit.Sts2.Core.Entities.Cards;
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
public sealed class EvolutionPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/EvolutionPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/EvolutionPower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  private Dictionary<string, int> _hitCounts = new();

  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
      DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
  {
      if (target == base.Owner && dealer != null && !dealer.IsPlayer)
      {
          _hitCounts.TryGetValue(dealer.Name, out int count);
          _hitCounts[dealer.Name] = count + 1;
      }
      await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
  }

  public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
      CardModel? cardSource, CardPlay? cardPlay)
  {
      if (target == base.Owner && dealer != null
                               && _hitCounts.TryGetValue(dealer.Name, out int count) && count > 0)
      {
          return -count;
      }
      return 0;
  }

}