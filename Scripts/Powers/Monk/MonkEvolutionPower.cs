using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 进化（对应塔1 MonkEvolutionPower）：每层使伙伴的攻击伤害 ×(1+层数)。
/// 倍率由伙伴侧在 <see cref="Rainworld.Scripts.Monster.Minions.FriendPetBase" /> 中读取。
/// </summary>
[RegisterPower]
public sealed class MonkEvolutionPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Evl128.png",
        BigIconPath: "res://Resource/Powers/Monk/Evl128.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
