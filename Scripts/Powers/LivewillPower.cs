using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
public sealed class LivewillPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/Oldsix48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Oldsix128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 
 private List<ModelId> cardList = new List<ModelId>();

 public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
 {
     modifiedCost = originalCost;
     if (card.Owner.Creature != base.Owner)
     {
         return false;
     }
     bool flag;
     switch (card.Pile?.Type)
     {
         case PileType.Hand:
         case PileType.Play:
             flag = true;
             break;
         default:
             flag = false;
             break;
     }
     if (!flag)
     {
         return false;
     }
     modifiedCost = Math.Max(originalCost - 1, 0);
     return true;
 }
 public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
 {
     if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Attack)
     {
         await PowerCmd.Remove(this);
     }
 }
 
 public override Decimal ModifyHpLostAfterOstyLate(
     Creature target,
     Decimal amount,
     ValueProp props,
     Creature? dealer,
     CardModel? cardSource)
 {
     return target != this.Owner ? amount : 0M;
 }
 public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
 {
     if (side == Owner.Side)
     {
             await PowerCmd.Decrement(this);
             
     }
 }
 
}
