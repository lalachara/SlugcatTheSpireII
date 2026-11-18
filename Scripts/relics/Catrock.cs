using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

/// <summary>
/// 典藏书签：卡牌奖励里<b>额外</b>多出 1 张其它蛞蝓猫颜色的牌。
/// <para>正常奖励仍是自己的颜色（3 张），这里只在选项末尾追加 1 张其它颜色的牌；
/// 不再像以前那样把其它颜色卡池并进整个奖励池（那样会变成 3 张随机颜色的牌）。</para>
/// </summary>
[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Catrock : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/bookmark.png",
        IconOutlinePath: "res://Resource/Relics/outline/bookmark.png",
        BigIconPath: "res://Resource/Relics/bookmark.png"
    );

    /// <summary>
    /// 卡牌奖励生成完 3 张之后，追加 1 张其它蛞蝓猫颜色的牌。
    /// 存在多个其它蛞蝓猫角色时会在这些颜色里随机；用原奖励的稀有度权重与随机源。
    /// </summary>
    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cards,
        CardCreationOptions options)
    {
        if (Owner != player)
            return false;

        // 只处理正常的卡牌奖励，且不处理明确禁止改池的场景
        if (!options.Flags.HasFlag(CardCreationFlags.IsCardReward))
            return false;
        if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
            return false;

        var currentPoolType = player.Character.CardPool.GetType();
        List<CardPoolModel> otherPools = Tools.GetOtherSlugcatCardPools(currentPoolType).ToList();
        if (otherPools.Count == 0)
            return false;

        // 只把「卡池」换成其它蛞蝓猫颜色，稀有度权重/随机源沿用原奖励；
        // 加 NoModifyHooks 防止抽这一张时再次触发本遗物（递归）。
        CardCreationOptions otherOptions = options
            .WithCardPools(otherPools)
            .WithFlags(options.Flags | CardCreationFlags.NoModifyHooks);

        CardCreationResult? extra = CardFactory.CreateForReward(player, 1, otherOptions).FirstOrDefault();
        if (extra == null)
            return false;

        cards.Add(extra);
        return true;
    }
}
