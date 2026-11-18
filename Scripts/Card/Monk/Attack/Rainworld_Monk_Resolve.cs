using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 迎刃而解：造成伤害，力量为负数时，每 1 点负力量为这张牌提升 ExtraDamage 点伤害。
/// 负力量本身还会让攻击伤害再降一次，因此净伤害为 CalculationBase + ExtraDamage × |力量| − |力量|。
/// </summary>
public class Rainworld_Monk_Resolve : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    /// <summary>负力量的点数；力量不为负数时为 0。</summary>
    private static decimal CalculateNegativeStrengthMultiplier(CardModel card, Creature creature)
    {
        if (card is not Rainworld_Monk_Resolve reslove)
            return 0m;
        var strength = reslove.Owner.Creature.GetPowerAmount<StrengthPower>();
        return strength < 0 ? -strength : 0m;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(9m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateNegativeStrengthMultiplier)
    ];

    public Rainworld_Monk_Resolve() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
