using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;

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
public sealed class TubePower : CustomPowerModel
{

  public override string? CustomPackedIconPath => "res://Resource/Powers/Tube48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Tube128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  
  public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
  {
    if (side == base.Owner.Side)
    {
      Flash();
      await CreatureCmd.GainBlock(base.Owner,  base.Amount, ValueProp.Unpowered, null);
    }
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
