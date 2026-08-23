﻿using MegaCrit.Sts2.Core.Context;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.RedCat;
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
public sealed class RedmushroomPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/RedMushRoom_128.png",
    BigIconPath: "res://Resource/Powers/RedCat/RedMushRoom_128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;

  public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
  {
    if (cardPlay.Card.Owner == Owner.Player && !cardPlay.Card.Keywords.Contains(RainworldKeywords.Corrupt)&&cardPlay.Card is not Rainworld_Redcat_Besame)
    {
      await SlugcatData.addCorruptValue(Owner.Player,-1);
     

    }

    await base.AfterCardPlayed(choiceContext, cardPlay);
  }
  public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
  {
    NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Player?.Creature!);
    if(!Owner.HasPower<FakedeathPower>())
    {
      creatureNode?.SetAnimationTrigger("Corrupt");
      // 进入假死/睡眠时
      if(Owner.Player?.Character is Slugcat slugcat)
          slugcat.SuppressHurtAnim = true;
      await PowerCmd.Apply<MushblockPower>(new BlockingPlayerChoiceContext(),Owner,10,Owner,null);
      if (LocalContext.IsMe(Owner))
        PlayerHurtVignetteHelper.Play();
      SfxCmd.Play("event:/Rainworld/sfx/workerror",volume:5F);

    }
  }
  

  public override async Task AfterRemoved(Creature oldOwner)
  {
    NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner.Player?.Creature!);
    if(!Owner.HasPower<FakedeathPower>())
    {
      if(!Owner.IsDead)
        creatureNode?.SetAnimationTrigger("Idle");
      if (!Owner.HasPower<InveteracyPower>())
      {
        if(Owner.HasPower<MushblockPower>())
          await CreatureCmd.GainBlock(Owner,Owner.GetPowerAmount<MushblockPower>(),ValueProp.Unpowered,null);
        await PowerCmd.Remove<MushblockPower>(Owner);
        await PowerCmd.Remove<CorruptStrengthPower>(Owner);
      }

      if(Owner.Player?.Character is Slugcat slugcat)
          slugcat.SuppressHurtAnim = false;
      

    }
  }
}