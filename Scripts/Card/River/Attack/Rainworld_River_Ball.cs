﻿
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Ball : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private int _currentDamage = 2;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Backtohand)};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(_currentDamage, ValueProp.Move),
        new("Special", 2m),
    ];

    public Rainworld_River_Ball() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var dmg = DynamicVars.Damage.BaseValue;
        var target = cardPlay.Target;
        var owner = Owner.Creature;

        await DamageCmd
            .Attack(dmg)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var selfDmgCmd = await DamageCmd
            .Attack(DynamicVars["Special"].BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(owner)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var selfResults = selfDmgCmd.Results.SelectMany(r => r);
        if (!selfResults.Any(r => r.UnblockedDamage > 0))
        {
            _currentDamage += 2;
            
            DynamicVars.Damage.BaseValue = _currentDamage;
            DynamicVars["Special"].BaseValue = _currentDamage;
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _currentDamage = 2;
            DynamicVars.Damage.BaseValue = _currentDamage;
            DynamicVars["Special"].BaseValue = _currentDamage;
        }
        return Task.CompletedTask;
    }
}