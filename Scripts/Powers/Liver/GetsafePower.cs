using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class GetsafePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/GetsafePower128.png",
        BigIconPath: "res://Resource/Powers/GetsafePower128.png"
    );
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
        { HoverTipFactory.FromPower<NimblePower>() };

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;



    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || amount <= 0)
            return;
        var nimble = Owner.Block;
        await CreatureCmd.LoseBlock(new ThrowingPlayerChoiceContext(), Owner,nimble,Owner);
        Flash();
        await PowerCmd.Apply<NimblePower>(new ThrowingPlayerChoiceContext(), Owner, nimble, Owner, null);
    }
}