﻿using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
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
public sealed class CantcontrolPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/UncontrolPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/UncontrolPower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  
  public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
  {
    if (cardPlay.Card.Owner == base.Owner.Player)
    {
      if (Owner.HasPower<RedmushroomPower>()&&(cardPlay.Card.Type==CardType.Attack||cardPlay.Card.Type==CardType.Skill))
      {
        await CardPileCmd.Draw(choiceContext, base.Amount, Owner.Player);
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, Owner, 2,ValueProp.Unpowered, Owner);
      }

    
      
    }

    await base.AfterCardPlayed(choiceContext, cardPlay);
  }
}