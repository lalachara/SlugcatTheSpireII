﻿using Godot;
 using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.River.Powers;

public class Rainworld_River_Driftingbottle : RiverCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public Rainworld_River_Driftingbottle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext,
            player: Owner,
            //filter: c => !c.Keywords.Contains(CardKeyword.Eternal),
            filter: null,
            source: this)).FirstOrDefault();

        if (selected != null)
        {
            if (selected.DeckVersion != null && Owner.Deck.Cards.Contains(selected.DeckVersion))
                await CardPileCmd.RemoveFromDeck(selected.DeckVersion);

            await CardCmd.Exhaust(choiceContext, selected);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}