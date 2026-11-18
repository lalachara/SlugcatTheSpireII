using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Forever : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<Rainworld_Liver_Twistfate>(true), HoverTipFactory.FromCard<Rainworld_Liver_Workerror>(true), HoverTipFactory.FromCard<Rainworld_Liver_Threeagree>(true) };

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/Eternal.png",
        IconOutlinePath: "res://Resource/Relics/outline/Eternal.png",
        BigIconPath: "res://Resource/Relics/Eternal.png"
    );

    public override async Task AfterObtained()
    {
        var options = new List<CardModel>
        {
            Owner.RunState.CreateCard<Rainworld_Liver_Twistfate>(Owner),
            Owner.RunState.CreateCard<Rainworld_Liver_Workerror>(Owner),
            Owner.RunState.CreateCard<Rainworld_Liver_Threeagree>(Owner),
        };
        foreach (CardModel card in options)
        {
            CardCmd.Upgrade(card);
        }

        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(), options, Owner, canSkip: true);

        if (chosenCard != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosenCard, PileType.Deck));
        }

        foreach (CardModel card in options)
        {
            if (card != chosenCard)
            {
                Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(Owner.NetId)
                    .CardChoices.Add(new CardChoiceHistoryEntry(card, wasPicked: false));
            }
        }
    }
}