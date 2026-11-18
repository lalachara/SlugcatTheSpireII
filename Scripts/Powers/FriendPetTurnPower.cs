using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 非蜥蜴类伙伴（猫崽 / 拾荒者等）的自动行动驱动：与 <see cref="FriendCabAttackPower" /> 完全相同的逻辑，
/// 但<b>永远不显示图标</b>——这些伙伴不希望出现「伙伴」buff。
/// <para>蜥蜴类伙伴仍然使用可见的 <see cref="FriendCabAttackPower" />（显示 mushblock 图标）。</para>
/// </summary>
[RegisterPower]
public sealed class FriendPetTurnPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/mushblock.png",
        BigIconPath: "res://Resource/Powers/Monk/mushblock.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>内部驱动能力：永不显示图标（猫崽、拾荒者身上不出现「伙伴」buff）。</summary>
    protected override bool IsVisibleInternal => false;

    /// <summary>
    /// 玩家回合结束（Early 阶段，早于 FriendDormantPower 的休眠倒计时结算）：
    /// 未休眠的伙伴自动攻击；目标优先带「优先攻击」的敌人，否则随机。
    /// </summary>
    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || !Owner.IsAlive)
            return;

        // 休眠中的伙伴不行动；复活当回合的早阶段 IsDormant 仍为 true，同样跳过
        if (Owner.GetPower<FriendDormantPower>() is { IsDormant: true })
            return;

        await FriendPetBase.PerformAutoTurn(Owner, choiceContext);
    }
}
