using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 腐化蜥蜴的形态能力（对应塔1 挂在紫蘑菇遗物上的 onExhaust 回血）：
/// 主人每消耗 1 张牌，腐化蜥蜴回复「1 + 协调层数」点生命；
/// 0 血永久眩晕期间，这条回血就是它重新加入战斗的唯一途径。
/// 能力图标同时充当腐化形态的状态显示（悬停描述随形态切换）。
/// </summary>
[RegisterPower]
public sealed class CorruptLizardPower : ModPowerTemplate
{
    /// <summary>塔1 HEAL_PER_EXHAUST：每次消耗回复的基础生命（实际再加上「协调」层数）。</summary>
    public const decimal HealPerExhaust = 1m;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/MushRoom128.png",
        BigIconPath: "res://Resource/Powers/MushRoom128.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>腐化形态：生命上限超过阈值。</summary>
    private bool IsRotForm => Owner.Monster is FCorruptLizard { IsRotForm: true };

    /// <summary>悬停描述随形态切换（普通形态 / 腐化形态）。</summary>
    public override LocString Description => new(
        "powers",
        IsRotForm ? $"{Id.Entry}.rotDescription" : $"{Id.Entry}.description");

    /// <summary>
    /// 主人消耗牌时回血（塔1 healFromExhaust = 1 + 协调层数）。
    /// 只响应主人的牌：多人模式下队友的消耗不会给它回血。
    /// </summary>
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card,
        bool causedByEthereal)
    {
        if (Owner.PetOwner == null || card.Owner != Owner.PetOwner)
            return;

        // 满血时不播治疗音：CreatureCmd.Heal 对满血目标没有效果
        bool damaged = Owner.CurrentHp < Owner.MaxHp;
        await CreatureCmd.Heal(Owner, HealPerExhaust + TuneAmount(), true);
        if (damaged)
            SfxCmd.Play(FCorruptLizard.Sfx.Heal);
    }

    /// <summary>
    /// 受击音：只有真的掉血才播（生命流失等非攻击伤害不算受击）。
    /// 眩晕期间不会被命中（<see cref="FriendDormantPower.ShouldAllowHitting" />），自然不会再响。
    /// </summary>
    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && (props & ValueProp.Unpowered) == 0 && result.UnblockedDamage > 0)
            SfxCmd.Play(FCorruptLizard.Sfx.Hurt);

        return Task.CompletedTask;
    }

    /// <summary>玩家身上的「协调」层数（塔1 tuneAmount；无则为 0）。</summary>
    private decimal TuneAmount() =>
        Owner.PetOwner?.Creature.GetPower<TunePower>()?.Amount ?? 0m;

    /// <summary>回合开始时同步形态：生命上限可能被吞噬以外的效果改变。</summary>
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == Owner.Side && Owner.Monster is FCorruptLizard lizard)
            lizard.SyncRotForm();

        return Task.CompletedTask;
    }
}
