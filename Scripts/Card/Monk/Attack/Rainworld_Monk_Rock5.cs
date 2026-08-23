using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using SlugcatTheSpireII.Scripts;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Rock5 : MonkCardModel
{
    private readonly HashSet<CardPoolModel> _playedColors = new();
    private int _colorfulCount;

    private const int energyCost = 5;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move)
    ];

    public Rainworld_Monk_Rock5() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(5)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card != this)
        {
            if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Colorful))
                _colorfulCount++;
            else
                _playedColors.Add(cardPlay.Card.VisualCardPool);
        }
        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _playedColors.Clear();
            _colorfulCount = 0;
        }
        await base.AfterPlayerTurnStart(choiceContext, player);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card == this)
        {
            var colorCount = _playedColors.Count + _colorfulCount;
            modifiedCost = Math.Max(0, originalCost - colorCount);
        }
        else
            modifiedCost = originalCost;
        return true;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}