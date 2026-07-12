﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Summon : RedCatCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Corrupt
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Corrupt),HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>() };


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Summon", 3m)
    ];

    public Rainworld_Redcat_Summon() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawnCards = await CardPileCmd.Draw(choiceContext, DynamicVars["Summon"].BaseValue, Owner);
            foreach (var card in drawnCards)
            {
                if (!card.Keywords.Contains(RainworldKeywords.Corrupt)&&!card.Keywords.Contains(RainworldKeywords.Cantcorrupt)&&!card.Keywords.Contains(RainworldKeywords.Immunity))
                   await Tools.CorruptCard(Owner, card);

            }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Summon"].UpgradeValueBy(1m);
    }
}