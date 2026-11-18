using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 矛造成的临时力量下降。原版通过 <see cref="TemporaryStrengthPower"/> 实现：
/// 施加时按 <see cref="TemporaryStrengthPower.IsPositive"/> 的符号扣除力量，目标回合结束时自动还原。
/// </summary>
public class SpearTempStrength : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Rainworld_Monk_Spear>();

    protected override bool IsPositive => false;

    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/tempstrength.png",
        BigIconPath: "res://Resource/Powers/tempstrength.png");

    public string? CustomIconPath => AssetProfile.IconPath;

    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}
