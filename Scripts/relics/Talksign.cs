using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Talksign : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel) };

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/Talksign.png",
        IconOutlinePath: "res://Resource/Relics/outline/Talksign.png",
        BigIconPath: "res://Resource/Relics/Talksign.png"
    );

    public override async Task AfterObtained()
    {

        if (Owner.Character is Slugcat)
        { 
            SlugcatData.addMaxWorkLevel(Owner,3);
           await  SlugcatData.addworklevel(Owner,3);
        }
    }
}