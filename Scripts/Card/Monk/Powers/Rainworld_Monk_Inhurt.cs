using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Inhurt : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 5m)
    ];
    public Rainworld_Monk_Inhurt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 层数即追加的生命流失量（5 / 升级 8），与卡面 {Special} 一致；
        // 触发阈值保持塔1 的固定 5，不随升级变化。
        InhurtPower power = await PowerCmd.Apply<InhurtPower>(choiceContext, Owner.Creature,
            DynamicVars["Special"].BaseValue, Owner.Creature, this);
        power.SetThreshold(InhurtPower.DefaultThreshold);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(3m);
    }
}