using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 伙伴：烈焰蜥蜴。增援1只烈焰蜥蜴伙伴。消耗。
/// </summary>
public class Rainworld_Monk_Firecab : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Firecab), HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Companioncab) };


    public Rainworld_Monk_Firecab() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FriendSummonCmd.Summon<FFireCab>(choiceContext, Owner,
            new FriendSummonOptions(Source: this, Position: FriendPositions.Next(Owner)));
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
