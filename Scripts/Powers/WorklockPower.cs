using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Powers;

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
public sealed class WorklockPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/WorkLock48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/WorkLock128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 

 public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
 {
     if (card.Owner.Creature == base.Owner)
     {
         await CreatureCmd.GainBlock(base.Owner, base.Amount*2, ValueProp.Unpowered, null);
         await PowerCmd.Apply<NimblePower>(base.Owner, Amount, base.Owner, null);
     }
 }
}
