using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：烈焰蜥蜴（对应塔1 FireCab：HP14、攻击4×2、每次行动后自身+1力量）。
/// </summary>
[RegisterMonster]
public sealed class FFireCab : FriendCabBase
{
    public override int MinInitialHp => 14;
    public override int MaxInitialHp => 14;

    protected override string SkinName => "red";

    protected override int AttackDamage => 4;

    /// <summary>攻击意图为多段（4×2），伤害数字随「协调」/力量实时刷新。</summary>
    protected override AbstractIntent CreateAttackIntent() =>
        new CabMultiAttackIntent(ComputeAttackDamage, 2);

    /// <summary>与上面 2 段意图一致：护甲面板按两段合计预计输出（塔1 intentTotalOutput）。</summary>
    protected override int IntentHitCount => 2;

    /// <summary>塔1 FireCab：对同一目标连击两次，行动后自身永久+1力量。</summary>
    public override async Task PerformCabAttack(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        Creature? target = targets.FirstOrDefault(t => t.IsAlive);
        if (target == null)
            return;

        PlayAttackSfx();
        await AttackOnce(choiceContext, target);
        if (target.IsAlive)
            await AttackOnce(choiceContext, target);

        await PowerCmd.Apply<StrengthPower>(choiceContext, Creature, 1m, Creature, null);
        RefreshCabIntents();
    }
}
