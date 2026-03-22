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
public sealed class TiredPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/Tired48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Tired128.png";
  
  public override PowerType Type => PowerType.Debuff;

  public override PowerStackType StackType => PowerStackType.Counter;

 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 

 public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
 {
     if (side == base.Owner.Side)
     {
         Flash();
         if (Owner.Player?.Character is Slugcat)
         {
             SlugcatField.GetSlugCatData[Owner].addworklevel(-Amount);
         }
     }
     await PowerCmd.Remove(this);

 }
}
