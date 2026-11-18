using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 识破：当你造成生命流失时，抽 Special 张牌。
/// 重复打出会叠层（SeethroughPower 为可叠层能力），每次抽更多张。
/// </summary>
public class Rainworld_Monk_Seethrough : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_Monk_Seethrough() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SeethroughPower>(choiceContext, Owner.Creature,
            DynamicVars["Special"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级只减 1 费
        EnergyCost.UpgradeBy(-1);
    }
}
