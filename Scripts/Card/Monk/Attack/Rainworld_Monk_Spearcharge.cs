using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 友谊与羁绊之矛：造成伤害，你每有1个伙伴，额外攻击1次。矛。
/// </summary>
public class Rainworld_Monk_Spearcharge : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<Rainworld_Liver_Spear>(IsUpgraded) , HoverTipFactory.FromKeyword(RainworldKeywords.Companion) };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Spear };

    /// <summary>伙伴数量乘数：每有1个伙伴，攻击次数 +1。</summary>
    private static decimal CountFriends(CardModel card, Creature? creature)
    {
        if (card is not Rainworld_Monk_Spearcharge)
            return 0m;
        return FriendPositions.Count(card.Owner);
    }

    /// <summary>
    /// 攻击次数变量：默认用 CalculationBase/CalculationExtra，
    /// 这里指向独立的 HitsBase / HitsExtra，避免与伤害的 CalculationBase 共用一个值。
    /// </summary>
    private sealed class HitCountVar : CalculatedVar
    {
        private readonly DynamicVar _hitsBase;
        private readonly DynamicVar _hitsExtra;

        public HitCountVar(DynamicVar hitsBase, DynamicVar hitsExtra) : base("CalculatedHits")
        {
            _hitsBase = hitsBase;
            _hitsExtra = hitsExtra;
        }

        protected override DynamicVar GetBaseVar() => _hitsBase;
        protected override DynamicVar GetExtraVar() => _hitsExtra;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            var hitsBase = new DynamicVar("HitsBase", 1m);
            var hitsExtra = new DynamicVar("HitsExtra", 1m);
            return
            [
                new CalculationBaseVar(9m),
                // 伤害不再随伙伴数增长，ExtraDamage 置 0 后 CalculatedDamage 恒为基础值。
                new ExtraDamageVar(0m),
                new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CountFriends),
                hitsBase,
                hitsExtra,
                new HitCountVar(hitsBase, hitsExtra).WithMultiplier(CountFriends)
            ];
        }
    }

    public Rainworld_Monk_Spearcharge() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 现算攻击次数：1 + 当前伙伴数，卡面数字与实际段数保持一致。
        var hits = (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(null);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
    }
}
