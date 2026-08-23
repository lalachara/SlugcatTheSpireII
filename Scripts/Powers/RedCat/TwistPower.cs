﻿using MegaCrit.Sts2.Core.Entities.Cards;
 using MegaCrit.Sts2.Core.Entities.Players;
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
public sealed class TwistPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/DiePower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/DiePower128.png"
  );
  protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
      new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Workreport),HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel),HoverTipFactory.FromPower<RedmushroomPower>()};

  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
  {
    if (player == Owner.Player && player.Character is Slugcat && (SlugcatData.GetWorkLevel(player) == 0||Owner.HasPower<RedmushroomPower>()))
    {
     await Tools.WorkReport(player);
    }

    await base.AfterPlayerTurnStart(choiceContext, player);
  }
}