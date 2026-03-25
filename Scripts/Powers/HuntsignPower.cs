using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;

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
public sealed class HuntsignPower : CustomPowerModel
{
  
  public override PowerType Type => PowerType.Debuff;
  private Boolean active = false;
  
  public override string? CustomPackedIconPath => "res://Resource/Powers/Huntbuff48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Huntbuff128.png";

  public override PowerStackType StackType => PowerStackType.Counter;
  
  

  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
    Creature? dealer, CardModel? cardSource)
  {
    if (target == base.Owner && dealer != null && (props!=ValueProp.Unpowered || cardSource is Omnislice))
    {
      if (dealer.Player.Character is Slugcat)
      {
        SlugcatField.GetSlugCatDataByCreature(Owner).addfood(Amount);

      }
    }
  }

  public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
  {
    if (side == CombatSide.Enemy)
    {
      
        await PowerCmd.Decrement(this);
        
    }
  }
  

}
