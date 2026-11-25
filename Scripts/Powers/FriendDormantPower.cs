using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Compat;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 伙伴休眠：蜥蜴类伙伴替主人挡伤倒下后不会退场，而是休眠数回合后满血复活（对应塔1 GreenCab 的休眠机制）。
/// 首次休眠 3 回合，此后每次倒下 +1 回合；休眠期间无法行动、不再被命中。
/// 剩余回合数显示在能力图标（Counter）上，悬停可查看描述。
/// </summary>
[RegisterPower]
public sealed class FriendDormantPower : ModPowerTemplate
{
    /// <summary>首次休眠 3 回合，之后每次倒下 +1（塔一原值为 2，本 mod 上调 1）。</summary>
    private const int BaseDormantTurns = 3;

    /// <summary>休眠变暗色（参考塔一 DORMANT_TINT）。</summary>
    private static readonly Color DormantTint = new(0.45f, 0.45f, 0.5f);

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Mushroomsleep128.png",
        BigIconPath: "res://Resource/Powers/Mushroomsleep128.png");

    public override PowerType Type => PowerType.Buff;

    /// <summary>休眠/守护等内部状态不再单独显示图标：伙伴身上只保留一个「伙伴」buff（FriendCabAttackPower）。
    /// 剩余复活回合数仍然显示在伙伴身上的圆形倒计时标签上。</summary>
    protected override bool IsVisibleInternal => false;

    /// <summary>休眠中切换为 Counter 以在图标上显示剩余回合；平时不显示数字（永久眩晕没有倒计时）。</summary>
    public override PowerStackType StackType =>
        IsDormant && !IsStunForever ? PowerStackType.Counter : PowerStackType.Single;

    /// <summary>悬停描述：休眠时显示剩余复活回合，平时显示能力说明；永久眩晕用独立文案。</summary>
    public override LocString Description => new(
        "powers",
        IsDormant
            ? (IsStunForever ? $"{Id.Entry}.stunForeverDescription" : $"{Id.Entry}.dormantDescription")
            : (IsStunForever ? $"{Id.Entry}.stunForeverIdleDescription" : $"{Id.Entry}.description"));

    protected override object InitInternalData() => new Data();

    private sealed class Data
    {
        public bool isDormant;
        public int dormantCount;
    }

    /// <summary>是否处于休眠状态。</summary>
    public bool IsDormant => GetInternalData<Data>().isDormant;

    /// <summary>
    /// 永久眩晕：倒下后不会随回合自动醒来，只能靠任意一次回血唤醒
    /// （对应塔1 AbstractFriend.STUN_FOREVER，腐化蜥蜴使用）。
    /// </summary>
    private bool IsStunForever => Owner.Monster is IStunForeverFriend;

    // ———— 死亡留场（官方复活型 power 的标准模式）————

    /// <summary>自己倒下时保留在战斗中（留场等待复活），其它生物不受影响。</summary>
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
        => creature != Owner;

    /// <summary>自己倒下后保留本 power（休眠倒计时依赖它）。</summary>
    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    /// <summary>
    /// 倒下时保留增益类 power（守护/行动等，复活后继续生效），清除负面与临时效果。
    /// 注意：这是全场共享的死亡查询（Hook.ShouldPowerBeRemovedOnDeath 会遍历战斗中所有 creature 的
    /// power / 遗物 / 药水 / 卡牌），必须先用 power.Owner 限定只处理自己的 power，
    /// 否则场上只要有一只伙伴，其它生物（例如实验体的「激怒」）死亡时也不会被清除 buff。
    /// </summary>
    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
        => power.Owner != Owner || power.Type == PowerType.Debuff || power is ITemporaryPower;

    /// <summary>休眠期间不再被命中，也不接受新的能力。</summary>
    public override bool ShouldAllowHitting(Creature creature)
        => creature != Owner || !IsDormant;

    // ———— 死亡与复活 ————

    // 0.113：AbstractModel.AfterDeath 新增了 CardModel? source 参数（致命伤害来源卡牌）
    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature,
        CardModel? source, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner || IsDormant)
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();
        bool stunForever = IsStunForever;
        // 先切换状态再 SetAmount：图标会按 Counter 重新刷新并显示休眠回合数。
        data.isDormant = true;
        if (stunForever)
        {
            // 永久眩晕没有倒计时，图标只表示「已眩晕」
            SetAmount(1);
        }
        else
        {
            SetAmount(BaseDormantTurns + data.dormantCount);
            data.dormantCount++;
        }
        SetDormantVisual(true);

        // 腐化蜥蜴受击至眩晕：播倒地音（普通蜥蜴只是休眠，没有专属音）
        if (stunForever)
            SfxCmd.Play(FCorruptLizard.Sfx.Fall);

        // 意图切为眩晕（STUN），并在身上显示复活倒计时（永久眩晕不显示数字）
        if (Owner.Monster is IFriendCabCombat cab)
        {
            cab.SetCabDormantIntent();
            // 骨架切到眩晕循环（yun / full_idle）：引擎死亡流程只触发 "Dead"，不会自己接眩晕动作
            cab.SetCabDormantAnim();
        }
        // 永久眩晕没有倒计时：不显示复活回合数（原先会显示 ∞）；其它蜥蜴照常显示数字
        if (stunForever)
            FriendDormantLabel.Hide(Owner);
        else
            FriendDormantLabel.Show(Owner, ((int)Amount).ToString());

        // 死亡流程会把本体改成不可交互（DisableInteractionForDeath 直接改 MouseFilter，
        // 绕过随从交互补丁）：这里恢复可悬停，让玩家能看到复活倒计时描述。
        NCreature? node = Owner.GetCreatureNode();
        if (node != null)
        {
            CreatureNodeCompat.SetInteractable(node, true);
            // 死亡动画播完后流程可能再次禁用交互/UI，延迟到动画结束后再补一次
            if (deathAnimLength > 0f)
                TaskHelper.RunSafely(ReEnableInteractionAfterDeathAnim(node, deathAnimLength));
        }

        return Task.CompletedTask;
    }

    /// <summary>死亡动画结束后再恢复一次交互与 UI（防止死亡流程后段再次禁用）。</summary>
    private async Task ReEnableInteractionAfterDeathAnim(NCreature node, float deathAnimLength)
    {
        if (!GodotObject.IsInstanceValid(node))
            return;
        SceneTree? tree = node.GetTree();
        if (tree == null)
            return;
        await node.ToSignal(tree.CreateTimer(deathAnimLength + 0.1f), SceneTreeTimer.SignalName.Timeout);

        if (!IsDormant || !GodotObject.IsInstanceValid(node))
            return;
        CreatureNodeCompat.SetInteractable(node, true);
        if (Owner.Monster is IFriendCabCombat cab)
        {
            cab.SetCabDormantIntent();
            // 倒地动画此时才播完，补一次眩晕动作，确保停在 yun / full_idle 上
            cab.SetCabDormantAnim();
        }
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 永久眩晕没有倒计时，不会随玩家回合结束自然复活
        if (!IsDormant || IsStunForever || side != Owner.Side)
            return;

        await TickDormant();
    }

    /// <summary>
    /// 推进一次休眠倒计时：玩家回合自然结束，或由「交互」卡强制执行「眩晕 -1」意图。
    /// 剩余回合数大于 1 时只递减并刷新图标数字，归零时满血复活。
    /// </summary>
    public async Task TickDormant()
    {
        // 永久眩晕只能靠回血唤醒，倒计时（含「交互」卡强制 -1）对它无效
        if (!IsDormant || IsStunForever)
            return;

        if (Amount > 1)
        {
            SetAmount(Amount - 1);
            FriendDormantLabel.Update(Owner, (int)Amount);
            return;
        }

        await Revive();
    }

    /// <summary>
    /// 立刻唤醒：供「治愈」卡等外部效果令休眠伙伴提前结束睡眠（满血复活，与倒计时自然醒来一致）。
    /// 未休眠时不做任何事。
    /// </summary>
    public async Task WakeFromDormant()
    {
        if (!IsDormant)
            return;
        await Revive();
    }

    /// <summary>倒计时结束：解除休眠并以满血复活。</summary>
    private async Task Revive()
    {
        EndDormant();   // 先恢复状态：放行治疗与命中，图标切回 Single，动画/意图一并还原
        await CreatureCmd.Heal(Owner, Owner.MaxHp - Owner.CurrentHp, true);
    }

    /// <summary>结束休眠状态并还原显示（清空数字、复原视觉、复活动画、恢复攻击意图）。</summary>
    private void EndDormant()
    {
        GetInternalData<Data>().isDormant = false;
        // SetAmount 在数值不变时不会触发刷新事件；此处 Amount 恰为 1（倒计时终点），
        // 借 0 中转一次，确保 NPower 重新读取 StackType 并把图标数字清空。
        SetAmount(0);
        SetAmount(1);
        SetDormantVisual(false);
        FriendDormantLabel.Hide(Owner);

        NCreature? node = Owner.GetCreatureNode();
        if (node != null)
        {
            // 触发 "Revive" 切回复活动画（FriendCabBase 注册的站立循环），并还原 UI 与悬停
            node.SetAnimationTrigger("Revive");
            CreatureNodeCompat.SetInteractable(node, true);
        }

        // 复活后立即把意图切回攻击；下一次玩家回合结束时由 FriendCabAttackPower 驱动攻击
        if (Owner.Monster is IFriendCabCombat cab)
            cab.SetCabAttackIntent();
    }

    /// <summary>休眠视觉：将伙伴本体变暗。</summary>
    private void SetDormantVisual(bool dormant)
    {
        Node2D? body = Owner.GetCreatureNode()?.Body;
        if (body == null)
            return;
        // 只改 RGB 通道、保留当前 alpha：与「悬停半透明」补丁（只动 modulate:a）互不干扰。
        Color current = body.Modulate;
        // 还原色取本体常态颜色：腐化形态等需要常态染色的伙伴不会在醒来时被刷回白色
        Color restore = (Owner.Monster as FriendPetBase)?.BaseBodyTint ?? Colors.White;
        body.Modulate = dormant
            ? new Color(DormantTint.R, DormantTint.G, DormantTint.B, current.A)
            : new Color(restore.R, restore.G, restore.B, current.A);
    }

    // ———— 兜底：被外力（治疗等）复活时同步解除休眠 ————

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Owner.Revived -= OnOwnerRevived;   // 保证只订阅一次
        Owner.Revived += OnOwnerRevived;
        return Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        oldOwner.Revived -= OnOwnerRevived;
        FriendDormantLabel.Hide(oldOwner);
        await base.AfterRemoved(oldOwner);
    }

    private void OnOwnerRevived(Creature creature)
    {
        // 正常复活路径（Revive()）已先解除休眠，这里兜底外力治疗复活的情况。
        if (IsDormant)
            EndDormant();
    }
}
