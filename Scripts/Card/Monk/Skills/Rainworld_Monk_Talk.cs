using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 茶话会：每有1个伙伴，抽1张牌。升级后额外抽1张。
/// </summary>
public class Rainworld_Monk_Talk : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 0m)
    ];

    public Rainworld_Monk_Talk() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Talk：抽 magicNumber 张 + 每有1个伙伴额外抽1张；无伙伴且未升级时不抽
        var draw = (int)DynamicVars["Special"].BaseValue + FriendPositions.Count(Owner);
        if (draw > 0)
            await CardPileCmd.Draw(choiceContext, draw, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
