using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 和平主义：打出当回合，你造成和受到的伤害变为1；之后你造成的伤害仍为1，直到你的下个回合结束。
/// 仅压制实际伤害数值（不含生命流失），且不改变敌人意图显示的数值。
/// </summary>
[RegisterPower]
public sealed class PeacePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Peace128.png",
        BigIconPath: "res://Resource/Powers/Monk/Peace128.png"
    );

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>打出当回合为 true：敌人对你的伤害也被压为1；进入下个玩家回合后变为 false。</summary>
    private bool _firstTurn = true;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 重复获得时重置压制窗口。
        _firstTurn = true;
        await base.AfterApplied(applier, cardSource);
    }

    /// <summary>输出侧：你造成的伤害变为1（生命流失除外）。不影响敌人意图的显示值。</summary>
    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == Owner && !IsHpLoss(props))
            return 1m;
        return decimal.MaxValue;
    }

    /// <summary>
    /// 受击侧：打出当回合你受到的伤害变为1（生命流失除外）。
    /// 走 HP 损失钩子（在格挡结算之后），因此敌人意图仍正常显示原伤害数值。
    /// </summary>
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 伙伴替主人挡下的攻击同样被压为 1
        if (FriendGuardPower.IsOwnerOrGuardingPet(Owner, target) && _firstTurn && !IsHpLoss(props) && amount >= 1m)
            return 1m;
        return amount;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        base.Flash();
        return Task.CompletedTask;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        base.Flash();
        return Task.CompletedTask;
    }

    // 时间线：打出回合压制双方伤害；下个玩家回合开始后只压制你造成的伤害；再下个玩家回合开始时移除。
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            if (_firstTurn)
                _firstTurn = false;
            else
                await PowerCmd.Remove(this);
        }
        await base.AfterSideTurnStart(side, participants, combatState);
    }

    /// <summary>生命流失（中毒、自伤等纯生命损失）不受影响：同时带有"不可格挡"与"无能力来源"两个标记。</summary>
    private static bool IsHpLoss(ValueProp props)
        => (props & (ValueProp.Unblockable | ValueProp.Unpowered)) == (ValueProp.Unblockable | ValueProp.Unpowered);
}
