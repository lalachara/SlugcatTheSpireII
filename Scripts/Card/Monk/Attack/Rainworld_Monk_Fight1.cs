using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 擒敌制胜：造成伤害，将1张「料敌机先」放入抽牌堆（对应塔1 Monk_Fight1）。
/// 两张牌互相生成，所以卡牌预览只做单层：用 FromCard 而不是 FromCardWithCardHoverTips，
/// 后者会把被预览卡自己的悬浮提示再展开，互相引用时会无限递归。
/// </summary>
public class Rainworld_Monk_Fight1 : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Monk_Fight2>(IsUpgraded) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move)
    ];

    public Rainworld_Monk_Fight1() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 强化过的「擒敌制胜」给强化过的「料敌机先」（同塔1：生成前把预览牌升级）。
        Rainworld_Monk_Fight2 card = CombatState!.CreateCard<Rainworld_Monk_Fight2>(Owner);
        if (IsUpgraded && card.IsUpgradable)
            CardCmd.Upgrade(card);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
