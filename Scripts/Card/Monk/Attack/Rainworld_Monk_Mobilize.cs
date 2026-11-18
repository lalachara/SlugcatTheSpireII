using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 战前动员：造成伤害，获得协调，你的下个回合开始前，失去协调。
/// </summary>
public class Rainworld_Monk_Mobilize : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<TunePower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new("Special", 2m)
    ];

    public Rainworld_Monk_Mobilize() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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

        // 塔1 Monk_Mobilize：本回合临时获得协调，下个回合开始时由 LoseTunePower 如数收回
        decimal tune = DynamicVars["Special"].BaseValue;
        await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature, tune, Owner.Creature, this);
        await PowerCmd.Apply<LoseTunePower>(choiceContext, Owner.Creature, tune, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(2m);
    }
}
