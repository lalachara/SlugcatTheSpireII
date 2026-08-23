using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using SlugcatTheSpireII.Scripts;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Bigsong : MonkCardModel
{
    private readonly HashSet<CardPoolModel> _playedColors = new();
    private int _colorfulCount;

    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private static decimal CalculateDamageMultiplier(CardModel card, Creature? creature)
    {
        if (card is not Rainworld_Monk_Bigsong bigsong)
            return 0m;
        return bigsong._playedColors.Count + bigsong._colorfulCount;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(4m),
        new ExtraDamageVar(2m),
        new("Special", 1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateDamageMultiplier)
    ];

    public Rainworld_Monk_Bigsong() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var colorCount = _playedColors.Count + _colorfulCount;
        var hitCount = (int)DynamicVars["Special"].BaseValue + colorCount;

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hitCount)
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

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
        DynamicVars["Extra"].UpgradeValueBy(1m);
    }
}