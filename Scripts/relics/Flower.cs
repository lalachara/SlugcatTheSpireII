using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts;
using Rainworld.Scripts.Potions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

/// <summary>
/// 轮回之花（对应塔1 Liver:FlowerAgain）：若你在战斗中失去过业力，
/// 在这场战斗的奖励里追加 1 个瓶装业力花。
/// 「是否失去过业力」由 <see cref="SlugcatData.setworklevel" /> 在战斗内业力下降时打标记，
/// 这里只负责在战斗奖励生成时消费标记并清空。
/// </summary>
[RegisterRelic(typeof(Rainworld_Monk_RelicPool))]
public sealed class Flower : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel),
        HoverTipFactory.FromPotion<Workflower>()
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/flower.png",
        IconOutlinePath: "res://Resource/Relics/outline/flower.png",
        BigIconPath: "res://Resource/Relics/flower.png"
    );

    /// <summary>
    /// 新一场战斗开始时清掉上一场遗留的标记，
    /// 避免「上一场丢了业力但没发战斗奖励」把瓶装业力花带到下一场。
    /// </summary>
    public override Task BeforeCombatStart()
    {
        if (Owner != null && SlugcatResources.Data.Get(Owner).KarmaLostInCombat)
            SlugcatResources.Data.Modify(Owner, d => d.KarmaLostInCombat = false);
        return Task.CompletedTask;
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || room is not CombatRoom)
            return false;
        if (!SlugcatResources.Data.Get(player).KarmaLostInCombat)
            return false;

        // 立刻清标记：同一次战斗的奖励即使被重复生成，也只会补一个瓶装业力花。
        SlugcatResources.Data.Modify(player, d => d.KarmaLostInCombat = false);
        Flash();
        rewards.Add(new PotionReward(ModelDb.Potion<Workflower>().ToMutable(), player));
        return true;
    }
}
