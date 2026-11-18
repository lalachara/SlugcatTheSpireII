using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 优先攻击：占位能力，暂无实际效果。
/// 设计目标（塔1）：伙伴生物的攻击意图优先选择该目标；场上同时只能有一个目标带有此能力。
/// </summary>
[RegisterPower]
public sealed class PriorityPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/youxian128.png",
        BigIconPath: "res://Resource/Powers/Monk/youxian128.png"
    );

    /// <summary>不叠层：仅作标记，无数字显示（与塔1 amount=-1 的表现一致）。</summary>
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
}
