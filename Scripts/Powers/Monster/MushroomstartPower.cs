﻿﻿using MegaCrit.Sts2.Core.Commands.Builders;
  using MegaCrit.Sts2.Core.Entities.Cards;
  using MegaCrit.Sts2.Core.Entities.Players;
  using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Monster;
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
public sealed class MushroomstartPower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Mushroomsleep128.png",
    BigIconPath: "res://Resource/Powers/Mushroomsleep128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    DamageResult result,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
  {
    
    if (target != Owner)
      return;
    if (result.UnblockedDamage == 0)
      return;
    if(Owner.Monster==null || Owner.Monster is not Mushroom mushroom)
      return;
    
    Mushroom monster = (Mushroom) Owner.Monster;
    await PowerCmd.Remove(this);
    monster.SetMoveImmediate(monster.WakeState);
    monster.MoveStateMachine.StateLog.Add(monster.WakeState);

  }

  public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
  {
    if(side!=Owner.Side)
      return;
    await CreatureCmd.GainMaxHp(Owner,(int)(Owner.MaxHp*0.1));
    if(Owner.Monster==null || Owner.Monster is not Mushroom mushroom)
      return;
    if (Amount <= 1)
    {
          Mushroom monster = (Mushroom) Owner.Monster;
          monster.NextMove.FollowUpState = monster.WakeState;

    }
    await PowerCmd.Decrement(this);

  }
  

  public override async Task AfterRemoved(Creature oldOwner)
  {
    if(Owner.Monster==null || Owner.Monster is not Mushroom mushroom)
      return;
    
    
    Mushroom monster;
    monster = (Mushroom) Owner.Monster;
    //SfxCmd.Play("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken");
    await CreatureCmd.TriggerAnim(Owner, "sleeptoidle", 0.6f);
    monster.IsAwake = true;

  }
  
}