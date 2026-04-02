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
public sealed class CatcomePower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/CatComing48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/CatComing128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 

 public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
 {
     if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Keywords.Contains(RainworldKeywords.Spear))
     {
         await CardPileCmd.Draw(context, Amount, base.Owner.Player);
     }
 }
 
}
