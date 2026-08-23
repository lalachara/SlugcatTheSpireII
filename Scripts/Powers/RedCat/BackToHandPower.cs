﻿using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;
[RegisterPower]
public class BackToHandPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/BacktohandPower128.png",
        BigIconPath: "res://Resource/Powers/RedCat/BacktohandPower128.png"
    );
  
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation location)
    {
        if (card.Owner.Creature != Owner)
            return location;
        if (location.pileType == PileType.Discard)
        {
            location.pileType = PileType.Hand;
            location.position = CardPilePosition.Top;
        }
        return location;
    }

    public override async Task AfterModifyingCardPlayResultLocation(
        CardModel card,
        CardLocation location)
    {
        if (card.Owner.Creature != Owner)
            return;
        await Tools.BackToHandAciton(Owner.Player, card);

        Flash();
        await PowerCmd.Decrement(this);
    }
}