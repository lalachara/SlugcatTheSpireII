using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

#nullable enable
[RegisterPower]
public sealed class InhurtPower : ModPowerTemplate
{
    /// <summary>塔1 InternalBreak.onAttack 的触发阈值：本次伤害不大于 5 时才追加。</summary>
    public const decimal DefaultThreshold = 5m;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/inhurt128.png",
        BigIconPath: "res://Resource/Powers/Monk/inhurt128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>触发阈值：Monk 版内伤为 5（塔1 原版），Future 版内伤为 7。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Threshold", DefaultThreshold)];

    /// <summary>当前生效的触发阈值（同时用于能力描述里的 {Threshold}）。</summary>
    public decimal Threshold => DynamicVars["Threshold"].BaseValue;

    /// <summary>覆盖阈值：同名牌叠加时以最后一次打出的卡为准。</summary>
    public void SetThreshold(decimal threshold) => DynamicVars["Threshold"].BaseValue = threshold;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner || !target.IsAlive || Amount <= 0)
            return;

        // 生命流失（包括本能力自己追加的那一次）不能再触发，否则会无限连锁。
        // 塔2 的生命流失固定同时带 Unblockable + Unpowered，用「等于 Unpowered」会漏掉组合标记。
        if ((props & (ValueProp.Unblockable | ValueProp.Unpowered)) ==
            (ValueProp.Unblockable | ValueProp.Unpowered))
            return;

        // 只对「本次伤害不大于阈值」的攻击追加；TotalDamage 是未扣格挡的原始伤害，与塔1 damageAmount 同口径。
        if (result.TotalDamage <= 0 || result.TotalDamage > Threshold)
            return;

        Flash();
        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
    }
}