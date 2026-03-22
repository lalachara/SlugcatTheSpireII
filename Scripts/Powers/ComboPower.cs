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
public sealed class ComboPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/Combo48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Combo128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  private bool skill = false, attack = false;
  
 // protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { (HoverTipFactory.FromPower<DoomPower>()) }; 

 public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
 {
     skill = false;
     attack = false;
 }

 public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
 {
     if (cardPlay.Card.Owner == base.Owner.Player )
     {
         if (cardPlay.Card.Type == CardType.Attack)
             attack = true;
         if(cardPlay.Card.Type == CardType.Skill)
             skill = true;
         if (skill & attack)
         {
             skill = false;
             attack = false;
             await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
             await PowerCmd.Apply<DexterityPower>(Owner, Amount, Owner, null);
         }

     }
 }
 
}
