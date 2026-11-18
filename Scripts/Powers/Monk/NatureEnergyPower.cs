using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 自然能量：让「协调」同时提升你造成的生命流失伤害，提升量等于协调层数。
/// 只统计你作为来源、目标不是自己的生命流失（无视格挡且不吃加成的伤害）。
/// </summary>
[RegisterPower]
public sealed class NatureEnergyPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/ziran.png",
        BigIconPath: "res://Resource/Powers/Monk/ziran.png"
    );

    public override PowerType Type => PowerType.Buff;

    /// <summary>只作为开关，不叠层；重复打出不会重复加成。</summary>
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner)
            return amount;
        if ((props & (ValueProp.Unblockable | ValueProp.Unpowered)) !=
            (ValueProp.Unblockable | ValueProp.Unpowered))
            return amount;

        decimal tune = Owner.GetPower<TunePower>()?.Amount ?? 0m;
        return tune > 0m ? amount + tune : amount;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }
}
