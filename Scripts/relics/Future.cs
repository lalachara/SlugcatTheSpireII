﻿﻿﻿﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Resource.Card;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Future : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/key.png",
        IconOutlinePath: "res://Resource/Relics/outline/key.png",
        BigIconPath: "res://Resource/Relics/key.png"
    );

    public override async Task AfterObtained()
    {
        var poolCards = ModelDb.CardPool<Rainworld_Future_CardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity != CardRarity.Token&&c.Rarity != CardRarity.Ancient)
            .ToList();

        if (poolCards.Count == 0)
            return;

        var rng = Owner.RunState.Rng.UpFront;
        var selected = poolCards.OrderBy(_ => rng.NextInt(0, int.MaxValue)).Take(3).ToList();

        var createCardMethod = Owner.RunState.GetType().GetMethod("CreateCard", 1, [typeof(Player)]);
        var options = selected.Select(c =>
        {
            var genericMethod = createCardMethod!.MakeGenericMethod(c.GetType());
            return (CardModel)genericMethod.Invoke(Owner.RunState, [Owner]);
        }).ToList();

        foreach (var card in options)
        {
            CardCmd.Upgrade(card);
        }

        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, Owner, canSkip: false);

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