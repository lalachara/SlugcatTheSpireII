using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.relics;

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
public sealed class ChuangPower : CustomPowerModel
{
  
  public override PowerType Type => PowerType.Debuff;
  private Boolean active = false;
  
  public override string? CustomPackedIconPath => "res://Resource/Powers/ChuangImg48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/ChuangImg128.png";

  public override PowerStackType StackType => PowerStackType.Counter;
  
  public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
    Creature target, CardModel? cardSource)
  {
    if (dealer != null && (dealer == base.Owner || dealer.PetOwner?.Creature == base.Owner) )
    {
      await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount,  ValueProp.Unpowered, null, null);
      active = true;
      if (getfireblood() && Owner.Side!=CombatSide.Player)
      {
        await PowerCmd.ModifyAmount(choiceContext,this, 1, Owner,null);
      }
      
    }

  }

  public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
  {
    if (side == CombatSide.Enemy)
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
