﻿using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_End : FutureCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1)
    ];

    public Rainworld_Future_End() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var xCostCards = ModelDb.AllCardPools
            .SelectMany(pool => pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.EnergyCost.CostsX && c is not FutureCardModel && c.Rarity != CardRarity.Token&& c.Rarity != CardRarity.Ancient)
            .ToList();

        if (xCostCards.Count == 0)
            return;

        CardModel chosenCard;

        if (IsUpgraded && xCostCards.Count >= 3)
        {
            var distinctCards = CardFactory.GetDistinctForCombat(Owner, xCostCards, 3,
                Owner.RunState.Rng.CombatCardGeneration);
            var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, distinctCards.ToList(), Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1, 1));
            chosenCard = selected.First();
        }
        else
        {
            var distinctCards = CardFactory.GetDistinctForCombat(Owner, xCostCards, 1,
                Owner.RunState.Rng.CombatCardGeneration);
            chosenCard = distinctCards.First();
        }

        await CardPileCmd.AddGeneratedCardToCombat(chosenCard, PileType.Hand, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}