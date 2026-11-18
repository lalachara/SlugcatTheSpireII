using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 伙伴的行动驱动：玩家回合结束时命令伙伴自动行动，图标显示在伙伴身上。
/// 目标优先带「优先攻击」(<see cref="PriorityPower" />) 的敌人，否则随机一个存活敌人；
/// 焦糖蜥蜴等 HitsAllEnemies 的伙伴攻击所有敌人。
/// </summary>
[RegisterPower]
public sealed class FriendCabAttackPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/mushblock.png",
        BigIconPath: "res://Resource/Powers/Monk/mushblock.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;


    /// <summary>
    /// 玩家回合结束（Early 阶段，一定早于 FriendDormantPower 的休眠倒计时结算）：
    /// 未休眠的伙伴自动攻击；目标优先带「优先攻击」的敌人，否则随机。
    /// </summary>
    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || !Owner.IsAlive)
            return;

        // 休眠中的伙伴不行动；复活当回合的早阶段 IsDormant 仍为 true，同样跳过——
        // 与玩家看到的「眩晕意图」一致，下一次玩家回合结束才恢复攻击。
        if (Owner.GetPower<FriendDormantPower>() is { IsDormant: true })
            return;

        // 目标选择与行动执行由伙伴基类的公共流程接管（非伙伴挂到本能力时为空操作）
        await FriendPetBase.PerformAutoTurn(Owner, choiceContext);
    }
}
