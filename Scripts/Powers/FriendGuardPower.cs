using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 伙伴守护：蜥蜴类伙伴替主人承受未被格挡的攻击伤害（对应塔1伙伴的 Protect）。
/// <para>全部走原版 hook，<b>不需要任何 Harmony 补丁</b>：</para>
/// <list type="number">
///   <item>玩家格挡由引擎自己先吃（和原版一致）；</item>
///   <item><see cref="ModifyHpLostAfterOstyLate" />（原版把众数修正分成 BeforeOsty / AfterOsty / Late 三个固定阶段，
///         菌壁、冰晶用的是 AfterOsty，Late 阶段必定排在它们之后）里，
///         按 <see cref="GuardOrder" /> 的顺序把「玩家即将失去的生命」切给伙伴：
///         每只最多吃它<b>当前生命值</b>那么多，切走的部分直接从玩家身上扣掉；</item>
///   <item><see cref="AfterModifyingHpLostAfterOsty" />（原版专门给这种钩子留的异步收尾）里，
///         把切出去的部分用「不可格挡 + 无能力来源」的生命流失打给伙伴
///         （伙伴本身没有格挡/减伤手段，这样也不会再触发承伤链自身）。</item>
/// </list>
/// <para>于是链条是：玩家格挡 → 玩家减伤（菌壁 / 冰晶 / 浴血…）→ ost → 伙伴（后增援先扛）→ 玩家，
/// 且「4 血伙伴吃 7 点」只会掉 4、剩下的 3 点才回到玩家身上，不会出现溢出直接砸玩家的老问题。</para>
/// </summary>
[RegisterPower]
public sealed class FriendGuardPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Mushroomsleep128.png",
        BigIconPath: "res://Resource/Powers/Mushroomsleep128.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>守护是伙伴的固有属性，不在 UI 上占用能力图标。</summary>
    protected override bool IsVisibleInternal => false;

    /// <summary>
    /// 承伤顺序：后增援的伙伴优先，其次是更早的伙伴；阵亡/休眠（IsDead）的伙伴不参与。
    /// 代偿与 UI（伙伴护甲面板）共用这一顺序，保证「谁先扛、之后顺延给谁」一致。
    /// </summary>
    public static List<Creature> GuardOrder(Player player) =>
        FriendPositions.All(player)
            .Reverse()
            .Where(pet => pet.IsAlive && pet.GetPower<FriendGuardPower>() != null)
            .ToList();

    /// <summary>
    /// 这次 HP 损失是否算作「该玩家的受伤」：他本人，或者替他承伤的单位
    /// —— 挂了守护的蜥蜴伙伴，以及原版 ost（<see cref="DieForYouPower" />）。
    /// 菌壁 / 冰晶等「你受到的伤害」减免用它判断，这样玩家身上的减伤在代偿之后依然生效。
    /// </summary>
    public static bool IsOwnerOrGuardingPet(Creature self, Creature target) =>
        target == self ||
        (target.PetOwner?.Creature == self &&
         (target.GetPower<FriendGuardPower>() != null || target.GetPower<DieForYouPower>() != null));

    /// <summary>本次伤害里「已切给伙伴、待结算」的列表：(伙伴, 伤害量)。</summary>
    private readonly List<(Creature Pet, decimal Amount)> _pending = [];

    /// <summary>
    /// 未被格挡、且已经过 ost 之后的「玩家将要失去的生命」：先按顺序切给伙伴，剩下的才由玩家承担。
    /// 每只伙伴最多吃它当前的生命值，所以不会出现「伙伴被打死、溢出直接砸玩家」。
    /// </summary>
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        Player? petOwner = Owner.PetOwner;
        if (petOwner == null || target != petOwner.Creature || amount <= 0m)
            return amount;

        // 生命流失（不可格挡 + 无能力来源）不代偿，只接攻击/能力造成的「伤害」
        if ((props & ValueProp.Unpowered) != 0)
            return amount;

        // 自己打自己（状态牌、遗物自伤等）不代偿
        if (dealer == petOwner.Creature)
            return amount;

        // 整条链只由顺序最靠前的一只伙伴执行一次，其它伙伴原样放行，避免重复扣减
        List<Creature> order = GuardOrder(petOwner);
        if (order.Count == 0 || order[0] != Owner)
            return amount;

        decimal remaining = amount;
        _pending.Clear();

        foreach (Creature pet in order)
        {
            if (remaining <= 0m)
                break;

            decimal hp = System.Math.Max(pet.CurrentHp, 0m);
            if (hp <= 0m)
                continue;

            decimal take = System.Math.Min(remaining, hp);
            _pending.Add((pet, take));
            remaining -= take;
        }

        // 返回玩家真正要承担的剩余伤害
        return remaining;
    }

    /// <summary>把上面切出去的伤害用「生命流失」打给伙伴（伙伴没有格挡/减伤手段，也不会再触发代偿）。</summary>
    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_pending.Count == 0)
            return;

        var pending = new List<(Creature Pet, decimal Amount)>(_pending);
        _pending.Clear();

        var context = new BlockingPlayerChoiceContext();

        foreach ((Creature pet, decimal take) in pending)
        {
            if (take <= 0m || !pet.IsAlive)
                continue;

            // dealer 传 null：这是「伙伴替主人承受」，不算玩家造成的生命流失，
            // 免得连带给「你造成生命流失」类能力（生息 / 本我 / 自然能量…）加数值。
            await CreatureCmd.Damage(context, pet, take, ValueProp.Unpowered | ValueProp.Unblockable, null);
        }

        await base.AfterModifyingHpLostAfterOsty();
    }
}
