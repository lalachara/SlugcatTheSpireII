using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 烈焰护卫队：失去你所有的伙伴，增援等量的烈焰蜥蜴伙伴。消耗。
/// </summary>
public class Rainworld_Monk_Firecabteam : MonkCardModel
{
    private const int energyCost = 5;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Firecab), HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Companioncab) };

    public Rainworld_Monk_Firecabteam() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只处理本 mod 的伙伴（拾荒者、蜥蜴、猫崽等）：
        // 原版异鸟、瓦皮、Pael's Legion 等宠物不响应这张牌——强行让它们退场会破坏原版逻辑，导致卡死
        List<Creature> friends = FriendPositions.All(Owner)
            .Where(friend => friend.Monster is FriendPetBase)
            .ToList();
        if (friends.Count == 0)
            return;

        var count = friends.Count;

        // 失去全部伙伴（休眠中的也一并退场）
        foreach (Creature friend in friends)
        {
            FriendPositions.LoseFriend(Owner, friend);
        }

        // 增援等量的烈焰蜥蜴（对应塔1 FireCabteam）
        for (var i = 0; i < count; i++)
        {
            await FriendSummonCmd.Summon<FFireCab>(choiceContext, Owner,
                new FriendSummonOptions(Source: this, Position: FriendPositions.Next(Owner)));
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
