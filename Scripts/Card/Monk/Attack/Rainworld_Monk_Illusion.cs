using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 折磨：令目标失去生命；强化后改为对所有敌人造成等量的生命流失。
/// 本场战斗你每次造成生命流失时，此牌数值增加 ExtraDamage 点（固定 1 点，不随强化提升）。
/// </summary>
public class Rainworld_Monk_Illusion : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>
    /// 本场战斗你造成生命流失的次数（与塔1 Inherent.hpLossCount 同口径：
    /// 只有你亲手造成的生命流失计入，中毒、灼烧这类没有来源的损失不算）。
    /// </summary>
    private static decimal CountHpLossInstances(CardModel card, Creature creature)
    {
        if (card is not Rainworld_Monk_Illusion illusion)
            return 0m;
        var dealer = illusion.Owner.Creature;
        return CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Count(e => e.Dealer == dealer
                && (e.Result.Props & ValueProp.Unblockable) != 0
                && (e.Result.Props & ValueProp.Unpowered) != 0
                && e.Result.UnblockedDamage > 0);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(5m),
        // 每次生命流失固定 +1，强化只提升基础值，不再叠高加值。
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unblockable | ValueProp.Unpowered)
            .WithMultiplier(CountHpLossInstances)
    ];

    /// <summary>强化后改为对全体敌人结算，目标类型随之变成全体。</summary>
    public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : base.TargetType;

    public Rainworld_Monk_Illusion() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        // 强化后：对每个敌人各结算一次，卡面数字与实际打出的数值一致。
        if (IsUpgraded)
        {
            var allValue = DynamicVars.CalculatedDamage.Calculate(null);
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, allValue,
                ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
            return;
        }

        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 按"此刻的累积加值"现算一次，卡面数字与实际打出的数值一定一致
        var value = DynamicVars.CalculatedDamage.Calculate(cardPlay.Target);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, value,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 强化只提升基础值（5 → 7），加值固定为1
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
    }
}
