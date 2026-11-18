using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
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

    /// <summary>
    /// 朝圣：提升业力上限（Monk 2 级，其余蛞蝓猫 1 级）。
    /// 描述文本里的 {Karma} 占位符与 OnSelect 的实际结算共用同一处规则，避免显示与结算不一致。
    /// </summary>
    public override LocString Description
    {
        get
        {
            LocString description = new("rest_site_ui", "OPTION_RAINWORLD_SHELTER.description");
            description.Add("Karma", SlugcatData.GetShelterKarmaLevel(Owner).ToString());
            return description;
        }
    }

    public override async Task<bool> OnSelect()
    {
        if (Owner.Character is Slugcat)
        {
            await SlugcatData.addMaxWorkLevel(Owner, SlugcatData.GetShelterKarmaLevel(Owner));
            SlugcatData.setchaosheng(Owner,true);
        }
            
        // 你的篝火事件逻辑
        return true;
    }
}