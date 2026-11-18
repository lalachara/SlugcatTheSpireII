using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 戮力同心：获得"戮力同心"能力（当你打出其它蛞蝓猫颜色的牌时，令随机敌人失去若干生命）。
/// </summary>
public class Rainworld_Monk_Allmywing : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 4m)
    ];

    public Rainworld_Monk_Allmywing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AllmywingPower>(choiceContext, Owner.Creature, DynamicVars["Special"].BaseValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(2m);
    }
}
