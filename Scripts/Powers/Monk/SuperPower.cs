using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Powers;

[RegisterPower]
public sealed class SuperPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Me128.png",
        BigIconPath: "res://Resource/Powers/Monk/Me128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 本我：按百分比提升自己造成的「生命流失」伤害（塔1 SuperPower.onAttackToChangeDamage
    /// 的 HP_LOSS 分支，倍率 1 + amount * 0.01）。
    /// </summary>
    /// <remarks>
    /// 生命流失在塔2 固定表现为 Unblockable + Unpowered 两个标记同时具备（与 NatureEnergyPower 同一判定），
    /// 不能用「含有 Unblockable 就排除」的写法，否则正好把要加成的伤害排掉了。
    /// </remarks>
    public override decimal ModifyHpLostAfterOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner)
            return amount;

        if ((props & (ValueProp.Unblockable | ValueProp.Unpowered)) !=
            (ValueProp.Unblockable | ValueProp.Unpowered))
            return amount;

        decimal scaled = amount * (1m + Amount * 0.01m);

        // 向上取整：1 点生命流失 +50% 要打 2 点，而不是 1.5 被引擎截断成 1。
        // 只有被放大时才取整，层数为 0（或异常为负）时保持原值，避免把本身带小数的流失值抬上去。
        return scaled > amount ? Math.Ceiling(scaled) : amount;
    }
}