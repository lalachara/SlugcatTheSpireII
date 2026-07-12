﻿using MegaCrit.Sts2.Core.Entities.Cards;
 using MegaCrit.Sts2.Core.Entities.Players;
 using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
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
public sealed class SubstratePower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/Sub128.png",
    BigIconPath: "res://Resource/Powers/RedCat/Sub128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
  {
    if (player == base.Owner.Player)
    {
      await CardPileCmd.AddGeneratedCardToCombat(
        base.CombatState.CreateCard<Rainworld_Redcat_Corrupt>(base.Owner.Player),
        PileType.Hand, base.Owner.Player);
    }
    await base.AfterPlayerTurnStart(choiceContext, player);
  }

  public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
  {
    if (cardPlay.Card is Rainworld_Redcat_Corrupt && cardPlay.Card.Owner == base.Owner.Player)
    {
      // Flash();
      // await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, cardPlay);
      await PowerCmd.ModifyAmount(choiceContext, this, 1, base.Owner, null);
    }
    await base.AfterCardPlayed(choiceContext, cardPlay);
  }
}