using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 播种：<b>纯显示能力</b>，没有任何游戏逻辑。
/// 「播种」每次打出都会往本场战斗的奖励里塞 1 组卡牌奖励（3 选 1），
/// 这个能力只是把「本场战斗额外获得了几组卡牌奖励」显示在玩家头上，
/// 层数 = 已经加了组数；实际加奖励仍由 <see cref="Card.Monk.Rainworld_Monk_Six" /> 完成。
/// </summary>
[RegisterPower]
public sealed class SeedPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Seed128.png",
        BigIconPath: "res://Resource/Powers/Monk/Seed128.png"
    );

    public override PowerType Type => PowerType.Buff;

    /// <summary>按「额外奖励组数」叠层，因此用 Counter；不显示负数。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;
}
