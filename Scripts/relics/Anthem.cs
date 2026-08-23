using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Anthem : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(300)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/anthem.png",
        IconOutlinePath: "res://Resource/Relics/outline/anthem.png",
        BigIconPath: "res://Resource/Relics/anthem.png"
    );

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(300, Owner);
        await PlayerCmd.GainGold(Owner.Gold, Owner);
    }
}