﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card;

namespace SlugcatTheSpireII.Scripts.Card.Monk.Attack;

public class Rainworld_Monk_Quitmagic :MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("Special", 5m)
    ];

    public Rainworld_Monk_Quitmagic() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var playerCount = CombatState.Players.Count;
        var divisor = (decimal)DynamicVars["Special"].BaseValue * playerCount;
        var loseAmount = cardPlay.Target.CurrentHp / divisor;

        // 生命流失要记来源（玩家），否则「当你造成生命流失」类能力（思考、识破、自然能量、幻象计数）都不会响应
        await CreatureCmd.Damage(choiceContext, cardPlay.Target,
            new DamageVar(loseAmount, ValueProp.Unblockable | ValueProp.Unpowered), Owner.Creature, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(-1m);
    }
}