using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
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
public sealed class BecausePower : CustomPowerModel
{

  public override string? CustomPackedIconPath => "res://Resource/Powers/Because48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Because128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  

  public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
  {
    if (side == base.Owner.Side)
    {
      if (Owner.Player.Character is Slugcat)
      {
        Flash();
        SlugcatField.GetSlugCatDataByCreature(Owner).addworklevel(-1);
      }
      else
      {
        await CreatureCmd.Kill(base.Owner);
      }

      
    }
  }
  

}
