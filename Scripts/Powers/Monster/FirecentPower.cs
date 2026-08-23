﻿﻿using MegaCrit.Sts2.Core.Commands.Builders;
  using MegaCrit.Sts2.Core.Entities.Cards;
  using MegaCrit.Sts2.Core.Entities.Players;
  using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
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
public sealed class FirecentPower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/FireCent128.png",
    BigIconPath: "res://Resource/Powers/FireCent128.png"
  );

  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;


  public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
  {
    if(Owner.Block<Amount)
      await CreatureCmd.GainBlock(Owner,Amount-Owner.Block, ValueProp.Unpowered, null);
    await base.AfterSideTurnStart(side, participants, combatState);
  }

  public override async Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
  {
    if(target!=Owner)
      return;

    if(Owner.Monster==null || Owner.Monster is not Firecent firecent)
      return;
    else
    {
      firecent.blocks--;
      switch (firecent.blocks)
      {
        case 3:
          firecent.SetSkin("hit1");
          await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
          break;
        case 2:
          firecent.SetSkin("hit2");
          await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
          break;
        case 1:
          firecent.SetSkin("hit3");
          await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
          break;
        case 0:
          firecent.SetSkin("hit4");
          await PowerCmd.Remove(this);
          break;
      }
    }
    await base.AfterBlockBroken(choiceContext, target, breaker);
  }
}