using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 恋旧：丢弃手牌中所有不带保留的牌，然后获得等量能量（对应塔1 Monk_Loveold）。
/// 塔1 里打出的牌要等 use() 返回后才离开手牌，所以那里把结算延后到动作队列；
/// 塔2 打出时这张牌已经在打出堆里，这里再显式排除自己以防万一。
/// </summary>
public class Rainworld_Monk_Loveold : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(CardKeyword.Retain) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 只用于描述里的 {Energy:energyIcons()} 图标；实际获得多少由弃掉的牌数决定。
        new EnergyVar(1)
    ];

    public Rainworld_Monk_Loveold() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 与塔1 同口径：retain 与「本回合保留」都算带保留（ShouldRetainThisTurn 同时覆盖两者）。
        List<CardModel> toDiscard = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card != this && !card.ShouldRetainThisTurn)
            .ToList();

        if (toDiscard.Count == 0)
            return;

        await CardCmd.Discard(choiceContext, toDiscard);
        await PlayerCmd.GainEnergy(toDiscard.Count, Owner);
    }

    protected override void OnUpgrade()
    {
        // 塔1：升级后 1 费 → 0 费
        EnergyCost.UpgradeBy(-1);
    }
}
