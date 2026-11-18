using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 屏息：获得能量，本回合无法抽牌。
/// </summary>
public class Rainworld_Monk_Cantbreathe : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2)
    ];

    public Rainworld_Monk_Cantbreathe() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        // 直接复用原版「战斗专注」的禁止抽牌能力（回合结束自动移除）。
        await PowerCmd.Apply<NoDrawPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 塔1：升级后能量 2 → 3
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
