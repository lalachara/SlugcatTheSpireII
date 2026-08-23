﻿using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Originalcorrupt : RedCatCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Corrupt
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>()};


    public Rainworld_Redcat_Originalcorrupt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => !c.Keywords.Contains(RainworldKeywords.Corrupt)&&!c.Keywords.Contains(RainworldKeywords.Cantcorrupt)&&!c.Keywords.Contains(RainworldKeywords.Immunity))
            .ToList();
        if (handCards.Count != 0)
        {
            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
            var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, handCards, Owner, prefs)).FirstOrDefault();
            if (selected != null)
               await Tools.CorruptCard(Owner, selected);
        }

        foreach (var card in PileType.Hand.GetPile(Owner).Cards.Where(c => c.Keywords.Contains(RainworldKeywords.Corrupt)))
            card.EnergyCost.AddThisTurnOrUntilPlayed(-1);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}