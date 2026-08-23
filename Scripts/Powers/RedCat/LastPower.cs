﻿using Godot;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
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
public sealed class LastPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/Last128.png",
    BigIconPath: "res://Resource/Powers/RedCat/Last128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;
  
  public override bool ShouldDie(Creature creature) =>creature!=Owner || !CombatState.Players.Any(p => p != Owner.Player && p.Creature.IsAlive && (!p.Creature.HasPower<FakedeathPower>()));
  public override async Task AfterPreventingDeath(Creature creature)
  {
    await SlugcatData.setworklevel(Owner.Player!,0);
    await CreatureCmd.Heal(Owner, 1);
    await PowerCmd.Apply<FakedeathPower>(new BlockingPlayerChoiceContext(),Owner,1,Owner,null);
    Flash();
    if(CombatState.CurrentSide == CombatSide.Player)
      PlayerCmd.EndTurn(Owner.Player!,true);
    await PowerCmd.Remove(this);
  }

  
  
}