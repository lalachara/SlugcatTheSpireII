using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Rainworld.Scripts.Card.Monk;

namespace Rainworld.Scripts.Powers;

public class BigrockTempStrength : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Rainworld_Monk_Bigrock>();
    protected override bool IsPositive => false;

    /// <summary>临时扣力量的统一图标（Resource/Powers/tempstrength.png）。</summary>
    public PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/tempstrength.png",
        BigIconPath: "res://Resource/Powers/tempstrength.png");

    public string? CustomIconPath => AssetProfile.IconPath;

    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}