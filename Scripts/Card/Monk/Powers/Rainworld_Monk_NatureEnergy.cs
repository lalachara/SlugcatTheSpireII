using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 自然能量：获得协调。协调会提升你造成的生命流失伤害。
/// </summary>
public class Rainworld_Monk_NatureEnergy : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<TunePower>(),
        HoverTipFactory.FromPower<NatureEnergyPower>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_Monk_NatureEnergy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature, DynamicVars["Special"].BaseValue,
            Owner.Creature, this);

        // 这张牌让「协调」获得提升生命流失伤害的能力，不叠层。
        if (!Owner.Creature.HasPower<NatureEnergyPower>())
        {
            await PowerCmd.Apply<NatureEnergyPower>(choiceContext, Owner.Creature, 1m,
                Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
