using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 交互：获得格挡，令所有蜥蜴类伙伴执行1次意图。
/// </summary>
public class Rainworld_Monk_Interact : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion)};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move)
    ];
    public override bool GainsBlock => true;

    public Rainworld_Monk_Interact() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 塔1 allLizardFriendTakeTurn：所有蜥蜴类伙伴立刻执行一次意图；
        // 休眠伙伴的意图就是「眩晕 -1」，所以强制执行时同样推进休眠倒计时（归零则满血复活）。
        // 其余伙伴（猫崽等）不响应交互（与塔1一致）。
        // 只驱动本 mod 的伙伴：原版宠物（异鸟、瓦皮、Pael's Legion 等）不响应交互
        foreach (Creature friend in FriendPositions.ModFriends(Owner))
        {
            if (friend.Monster is not IFriendCabCombat)
                continue;
            if (friend.GetPower<FriendDormantPower>() is { IsDormant: true } dormant)
            {
                await dormant.TickDormant();
                continue;
            }
            await FriendPetBase.PerformAutoTurn(friend, choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
