using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

/// <summary>
/// 腐化胚胎（对应塔1 Monk_Purplemushroom「紫色菌块」）：战斗开始时增援 1 只腐化蜥蜴。
/// 吞噬、成长、眩晕、挡伤都在伙伴自己身上（见 <see cref="FCorruptLizard" />），
/// 这个遗物只负责开战时把它叫出来。塔1 的 onExhaust 回血由 <see cref="CorruptLizardPower" /> 接管。
/// </summary>
[RegisterRelic(typeof(Rainworld_Monk_RelicPool))]
public sealed class Mushegg : ModRelicTemplate
{
    /// <summary>塔1 为 BOSS 层级；塔2 没有 Boss 稀有度，用最高层级的先古遗物占位。</summary>
    public override RelicRarity Rarity => RelicRarity.Rare;

    /// <summary>本遗物会在战斗中生成宠物（对齐原版 BoundPhylactery/Byrdpip 的声明）。</summary>
    public override bool SpawnsPets => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        // 腐化蜥蜴自身的数值与形态说明（生命 = 10 + 吞噬来的生命，意图随形态变化）
        // 直接按注册表/本地化构造：FromKeyword 在某些加载顺序下会拿到空 tip，这里做兜底
        RotcabHoverTip(),
        HoverTipFactory.FromKeyword(RainworldKeywords.Companion),
        HoverTipFactory.FromKeyword(RainworldKeywords.Companioncab)
    ];

    /// <summary>
    /// 「腐化蜥蜴」关键字的悬停提示：
    /// 优先走 RitsuLib 的关键字注册表，注册表拿不到时直接用 <c>card_keywords</c> 表构造，
    /// 保证遗物悬停里一定能看到这条说明。
    /// </summary>
    private static IHoverTip RotcabHoverTip()
    {
        string keywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(RainworldKeywords.Rotcab));

        try
        {
            return ModKeywordRegistry.CreateHoverTip(keywordId);
        }
        catch (Exception)
        {
            return new HoverTip(
                new LocString("card_keywords", $"{keywordId}.title"),
                new LocString("card_keywords", $"{keywordId}.description"));
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/mushegg.png",
        IconOutlinePath: "res://Resource/Relics/outline/mushegg.png",
        BigIconPath: "res://Resource/Relics/mushegg.png"
    );

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player == base.Owner && combatState.RoundNumber == 1)
        {
            Flash();
            await FriendSummonCmd.Summon<FCorruptLizard>(choiceContext, Owner,
                new FriendSummonOptions(Source: null, Position: FriendPositions.Next(Owner)));
        }
    }
}
