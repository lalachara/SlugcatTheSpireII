using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 彩虹桥：造成伤害，并为随机 1 张手牌添加彩虹。
/// 本场战斗内每打出一张彩虹牌（含本牌自己），此牌伤害就额外增加 ExtraDamage。
/// </summary>
public class Rainworld_Monk_Colorfulbridge : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>
    /// 本场战斗内已打出的彩虹牌数量。战斗开始时卡牌会从卡组克隆一份，
    /// 因此这个计数天然只属于本场战斗，战斗结束随克隆一起丢弃。
    /// </summary>
    private int _colorfulPlayedThisCombat;

    /// <summary>本牌自带彩虹，所以打出它自己也会计入成长。</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Colorful };

    /// <summary>伤害成长乘数：本场战斗内每打出 1 张彩虹牌算 1 层。</summary>
    private static decimal CountColorfulPlayed(CardModel card, Creature? creature)
        => card is Rainworld_Monk_Colorfulbridge bridge ? bridge._colorfulPlayedThisCombat : 0m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(8m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CountColorfulPlayed)
    ];

    public Rainworld_Monk_Colorfulbridge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 从还没有彩虹的手牌里随机挑 1 张；手牌都带彩虹时没有可添加的目标。
        List<CardModel> candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(card => !card.Keywords.Contains(RainworldKeywords.Colorful))
            .ToList();
        CardModel? picked = Owner.RunState.Rng.Shuffle.NextItem(candidates);
        picked?.AddKeyword(RainworldKeywords.Colorful);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 自己打出的彩虹牌（包括这张牌本身）都会让本场战斗的成长层数 +1。
        if (cardPlay.Card.Owner == Owner && cardPlay.Card.Keywords.Contains(RainworldKeywords.Colorful))
            _colorfulPlayedThisCombat++;

        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
    }
}
