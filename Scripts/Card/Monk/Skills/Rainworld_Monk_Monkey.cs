using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 伙伴：拾荒者。获得金币，并增援1只拾荒者伙伴。消耗。
/// </summary>
public class Rainworld_Monk_Monkey : MonkCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Monkey) };

    /// <summary>基础金币（与塔1 Monk_Monkey 一致）。</summary>
    private const decimal BaseGold = 10m;

    /// <summary>升级后额外增加的金币。</summary>
    private const decimal UpgradeGoldBonus = 10m;

    /// <summary>GoldVar 的固定名字，DynamicVars 索引用它。</summary>
    private const string GoldName = "Gold";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar((int)BaseGold)
    ];

    public Rainworld_Monk_Monkey() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Monkey：先拿钱，再叫出拾荒者
        // （拾荒者在回合结束时会看「当时手里的金币」决定递不递援助牌，所以先加金币）
        //
        // 金额直接由「基础值 + 是否升级」算出，不再从 DynamicVars 读：
        // 卡面数值由 GoldVar 提供，一旦动态变量在克隆 / 组件系统里被重置，
        // IntValue 会取到 0 —— 表现就是「卡牌的金币效果没生效」。
        decimal gain = BaseGold + (IsUpgraded ? UpgradeGoldBonus : 0m);
        int before = Owner.Gold;

        await PlayerCmd.GainGold(gain, Owner);

        if (gain > 0m && Owner.Gold <= before)
        {
            // 兜底：万一有别的钩子把「获得金币」清零（Hook.ModifyGoldGained 返回 0 之类），
            // 直接补足，保证卡面承诺多少就真的到手多少
            Owner.Gold = before + (int)gain;
        }

        // 增援1只拾荒者（顺序超过5只时由 FriendPositions 自动错落站位）
        await FriendSummonCmd.Summon<FMonkey>(choiceContext, Owner,
            new FriendSummonOptions(Source: this, Position: FriendPositions.Next(Owner)));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Gold.UpgradeValueBy(UpgradeGoldBonus);
    }
}
