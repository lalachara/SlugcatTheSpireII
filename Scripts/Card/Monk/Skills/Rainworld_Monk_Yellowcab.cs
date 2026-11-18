using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 伙伴：金蜥蜴。增援若干只金蜥蜴伙伴。消耗。
/// </summary>
public class Rainworld_Monk_Yellowcab : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Goldcab), HoverTipFactory.FromKeyword(RainworldKeywords.Companion), HoverTipFactory.FromKeyword(RainworldKeywords.Companioncab) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m)
    ];

    public Rainworld_Monk_Yellowcab() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var count = (int)DynamicVars["Special"].BaseValue;
        for (var i = 0; i < count; i++)
        {
            await FriendSummonCmd.Summon<FYellowCab>(choiceContext, Owner,
                new FriendSummonOptions(Source: this, Position: FriendPositions.Next(Owner)));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
