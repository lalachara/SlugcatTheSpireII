using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
public sealed class SleepPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/SleepBuff48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/SleepBuff128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;
  

 public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
 {
     if (side == CombatSide.Enemy)
     {
         if(Owner.Player?.Character is not Slugcat)
             return;
         SlugcatField.GetSlugCatDataByCreature(Owner).addworklevel(1);
         await PowerCmd.Remove(this);
     }
 }

 public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
 {
     if (target == base.Owner && dealer != null && _.UnblockedDamage>0)
     {
         await PowerCmd.Remove(this);

     }
 }
 
 
}
