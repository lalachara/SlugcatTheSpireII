using MegaCrit.Sts2.Core.Entities.Players;
using Rainworld.Scripts;
using STS2RitsuLib.Scaffolding.Content;

namespace SlugcatTheSpireII.Scripts.patches.CampRoomPatch;

public class RainworldShelterOption : ModRestSiteOptionTemplate
{
    public override string OptionId => "RAINWORLD_SHELTER";

    public override RestSiteOptionAssetProfile AssetProfile => new(
        IconPath: "res://Resource/other/Camp1.png"
    );

    public RainworldShelterOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        if (Owner.Character is Slugcat)
        {
            SlugcatData.addMaxWorkLevel(Owner,1);
            SlugcatData.setchaosheng(Owner,true);
        }
            
        // 你的篝火事件逻辑
        return true;
    }
}