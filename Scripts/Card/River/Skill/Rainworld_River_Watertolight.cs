﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Watertolight : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<CorrodePower>(), HoverTipFactory.FromKeyword(RainworldKeywords.Lightning) };
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("Corrode", 3m)
    ];

    public Rainworld_River_Watertolight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var owner = Owner.Creature;

        var corrodePower = owner.GetPower<CorrodePower>();
        if (corrodePower == null)
            return;

        var corrodeAmount = (int)corrodePower.Amount;
        await PowerCmd.Remove(corrodePower);

        await PowerCmd.Apply<CorrodePower>(choiceContext, cardPlay.Target, corrodeAmount, Owner.Creature, this);

        var threshold = (int)DynamicVars["Corrode"].BaseValue;
        var lightningCount = corrodeAmount / threshold;
        for (int i = 0; i < lightningCount; i++)
        {
            Tools.Yinlei(cardPlay, choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Corrode"].UpgradeValueBy(-1m);
    }
}