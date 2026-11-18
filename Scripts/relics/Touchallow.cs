using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts.Card.Future;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Touchallow : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/momo.png",
        IconOutlinePath: "res://Resource/Relics/outline/momo.png",
        BigIconPath: "res://Resource/Relics/momo.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromCard<Rainworld_Future_Ohcat>(true)
    };

    public override async Task AfterObtained()
    {
        var card = Owner.RunState.CreateCard<Rainworld_Future_Ohcat>(Owner);
        CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }
}