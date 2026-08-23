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
public sealed class SpreadPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/Spread128.png",
    BigIconPath: "res://Resource/Powers/RedCat/Spread128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

 // 修改前
  public override PowerStackType StackType => PowerStackType.Counter;
  

  public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
  {
    if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Keywords.Contains(RainworldKeywords.Corrupt)&&!cardPlay.Card.Keywords.Contains(RainworldKeywords.Cantcorrupt)&&!cardPlay.Card.Keywords.Contains(RainworldKeywords.Immunity))
    {
      var handPile = PileType.Hand.GetPile(base.Owner.Player);
      foreach (var card in handPile.Cards)
      {
        if (!card.Keywords.Contains(RainworldKeywords.Corrupt))
        {
          await Tools.CorruptCard(Owner.Player, card);
          break;
        }
      }
    }
    await base.AfterCardPlayed(choiceContext, cardPlay);
  }


}