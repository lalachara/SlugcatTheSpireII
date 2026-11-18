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
/// 万物竞发：获得格挡，令所有处于眩晕的伙伴结束眩晕意图，并令所有伙伴执行2次意图。
/// 与「交互」只驱动蜥蜴类不同，本牌对所有种类的伙伴（蜥蜴、猫崽、拾荒者）都生效。
/// </summary>
public class Rainworld_Monk_Wake : MonkCardModel
{
    /// <summary>每个伙伴执行意图的次数。</summary>
    private const int TurnCount = 2;

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion) };

    public Rainworld_Monk_Wake() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 唤醒：所有处于眩晕（休眠）的伙伴满血复活，意图切回攻击（同「治愈」的结束眩晕流程）。
        List<Creature> friends = FriendPositions.All(Owner).ToList();
        foreach (Creature friend in friends)
        {
            if (friend.GetPower<FriendDormantPower>() is { IsDormant: true } dormant)
                await dormant.WakeFromDormant();
        }

        // 竞发：所有伙伴连续执行2次意图。中途阵亡或离场（如拾荒者金币不足会自行离开）就不再继续。
        foreach (Creature friend in friends)
        {
            for (var turn = 0; turn < TurnCount; turn++)
            {
                if (!friend.IsAlive || !FriendPositions.All(Owner).Contains(friend))
                    break;
                await FriendPetBase.PerformAutoTurn(friend, choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
