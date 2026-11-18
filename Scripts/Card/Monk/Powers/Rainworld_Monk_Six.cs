using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 播种：在本场战斗奖励中额外添加 1 组卡牌奖励（3 选 1）。
/// 升级后该组奖励中第一张可强化的牌会被直接强化。
/// 同时叠 1 层纯显示用的 <see cref="SeedPower" />（头上图标显示「本场战斗额外获得 N 组卡牌奖励」）。
/// </summary>
public class Rainworld_Monk_Six : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    public Rainworld_Monk_Six() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState?.RunState.CurrentRoom is not CombatRoom room)
            return;

        // 没有"指定奖励组"的接口，标准做法就是造一组卡牌奖励直接塞进房间奖励里
        // （原版第二组卡牌奖励走的是同一套，CardReward 里已经 roll 好三张牌）。
        // 战斗结算时界面会把房间奖励原样快照进去，战斗中加进去的组会正常显示
        var reward = new CardReward(CardCreationOptions.ForRoom(Owner, room.RoomType), 3, Owner, null);
        reward.Populate();
        if (!reward.IsPopulated)
            return;

        if (IsUpgraded)
        {
            // 升级版：把其中第一张还能强化的牌直接强化（诅咒、状态这类不可强化的跳过）
            UpgradeFirstUpgradable(reward);
            // 万一以后奖励被重roll，也保证"至少有 1 张强化过的牌"
            reward.AfterGenerated += () => UpgradeFirstUpgradable(reward);
        }
        room.AddExtraReward(Owner, reward);

        // 纯显示能力：层数 = 本场战斗额外获得的卡牌奖励组数
        await PowerCmd.Apply<SeedPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    /// <summary>把奖励中第一张可强化的牌直接强化。</summary>
    private static void UpgradeFirstUpgradable(CardReward reward)
    {
        foreach (var card in reward.Cards)
        {
            if (!card.IsUpgradable)
                continue;
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
            break;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级只改变卡面说明（奖励中至少有 1 张强化过的牌），没有数值变化
    }
}
