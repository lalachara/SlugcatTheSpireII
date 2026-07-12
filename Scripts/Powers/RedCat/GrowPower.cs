using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.RedCat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

#nullable enable
[RegisterPower]
public sealed class GrowPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/Grow.png",
        BigIconPath: "res://Resource/Powers/RedCat/Grow.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player creator)
    {
        if (creator == null || creator.Creature != Owner)
            return;
        if (card.Type != CardType.Status || card is Rainworld_Redcat_Corrupt)
            return;

        Flash();
        await Tools.CorruptCard(creator, card);
        await PowerCmd.Apply<GrowPower>(new BlockingPlayerChoiceContext(), Owner, -1, Owner, null);
        await PowerCmd.Decrement(this);
    }
}