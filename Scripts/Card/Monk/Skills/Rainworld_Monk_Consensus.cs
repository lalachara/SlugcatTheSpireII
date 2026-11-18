using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 达成共识：你每有1个伙伴，获得1点能量。消耗。
/// </summary>
public class Rainworld_Monk_Consensus : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    public Rainworld_Monk_Consensus() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Consensus：每有1个伙伴，获得1点能量
        var friends = FriendPositions.Count(Owner);
        if (friends <= 0)
            return;
        await PlayerCmd.GainEnergy(friends, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
