﻿﻿﻿﻿using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

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
[RegisterPower]
public sealed class SleepPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/SleepBuff128.png",
    BigIconPath: "res://Resource/Powers/SleepBuff128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;


  public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
  {
      NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Player?.Creature!);
      if(Owner.Player?.Character is Slugcat slugcat)
          slugcat.SuppressHurtAnim = true;
      creatureNode?.SetAnimationTrigger("Sleep");
  }
  

  public override async Task AfterSideTurnEnd(
      PlayerChoiceContext choiceContext,
      CombatSide side,
      IEnumerable<Creature> participants)	{
     if (side == CombatSide.Enemy)
     {
         if(Owner.Player?.Character is not Slugcat || Owner.Player.GetRelic<Redmushroom>()!=null)
             return;
         if (SlugcatData.GetMaxWorkLevel(Owner.Player) == SlugcatData.GetWorkLevel(Owner.Player))
         {
             await PowerCmd.Apply<WorklockPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
             await CreatureCmd.Heal(Owner,Owner.MaxHp-Owner.CurrentHp,true);
         }
         else
             await  SlugcatData.addworklevel(Owner.Player, 1);

         if (SlugcatData.getCorruptValue(Owner.Player) == 5)
             await SlugcatData.setCorruptValue(Owner.Player, -5);
         await PowerCmd.Remove<RedmushroomPower>(Owner);
     }
 }

 public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
 {
     if(player!=Owner.Player)
         return;
     
     if (Owner.Player?.GetRelic<Liver_Sleeppotion>() != null)
     {
         await CardPileCmd.Draw(choiceContext, 2, Owner.Player);
         await PlayerCmd.GainEnergy((int)2, Owner.Player);
         //Owner.Player.GetRelic<Liver_Sleeppotion>().sleepthisturn = true;
     }
     await PowerCmd.Remove(this);

 }
 

 public override async Task AfterRemoved(Creature oldOwner)
 {
     if (Owner.Player?.Character is Slugcat slugcat)
     {
         slugcat.SuppressHurtAnim = false;
         NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Player?.Creature!);
         if(!Owner.IsDead)
            creatureNode?.SetAnimationTrigger("Idle");
     }
     
     await base.AfterRemoved(oldOwner);
 }

 public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
 {
     if (target == base.Owner && dealer != null && _.UnblockedDamage>0&&Owner.Player?.GetRelic<Liver_Sleeppotion>()==null)
     {
         await PowerCmd.Remove(this);

     }
 }
 
 
}