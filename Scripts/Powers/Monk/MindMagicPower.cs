using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 精神附魔：本回合拥有者造成伤害时，施加者对其目标额外造成等同于层数的体力流失。
/// 图标显示在被附魔的盟友身上（本回合结算标记）。
/// </summary>
[RegisterPower]
public sealed class MindMagicPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/mindmagic.png",
        BigIconPath: "res://Resource/Powers/Monk/mindmagic.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>不同玩家施加的精神附魔分别结算，互不干扰。</summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;


    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || result.TotalDamage <= 0)
            return;
        if (!Owner.IsAlive || !target.IsAlive)
            return;

        // 只对敌方目标生效，且不把生命流失本身当成新的伤害来源。
        if (target == Owner || target.Side == Owner.Side)
            return;
        if ((props & (ValueProp.Unblockable | ValueProp.Unpowered)) ==
            (ValueProp.Unblockable | ValueProp.Unpowered))
            return;

        // 施加者倒下后不再结算；允许「给自己上」（单人 / 自我附魔）——
        // 生命流失本身带 Unblockable|Unpowered，上面的判定已经把它挡掉，不会自我循环。
        Creature? applier = Applier;
        if (applier != null && !applier.IsAlive)
            return;

        await CreatureCmd.Damage(choiceContext, target, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, applier);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            await PowerCmd.Remove(this);

        await base.AfterSideTurnEnd(choiceContext, side, participants);
    }
}
