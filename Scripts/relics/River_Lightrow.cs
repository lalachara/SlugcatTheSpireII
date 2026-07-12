using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(Rainworld_River_RelicPool))]
public sealed class River_Lightrow : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/LightRod.png",
        IconOutlinePath: "res://Resource/Relics/outline/LightRod.png",
        BigIconPath: "res://Resource/Relics/LightRod.png"
    );
}