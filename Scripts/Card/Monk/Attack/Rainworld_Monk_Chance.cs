using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 把握时机：造成伤害，你使用的下几张带有保留的牌不耗能。
/// </summary>
public class Rainworld_Monk_Chance : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        new("Special", 1m)
    ];

    public Rainworld_Monk_Chance() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<ChancePower>(choiceContext, Owner.Creature, DynamicVars["Special"].BaseValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
