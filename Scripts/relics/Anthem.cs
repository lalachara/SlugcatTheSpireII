using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Anthem : ModRelicTemplate
{

#if STS2_0_113_0 || STS2_0_114_0
    /// <summary>快速模式（Blitz）没有金币概念：赞美诗只加金币，与原版金币遗物一致，不在该模式生成。</summary>
    protected override bool IsAllowedInternal(IRunState runState) =>
        runState.GameMode != GameMode.Blitz;
#endif
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