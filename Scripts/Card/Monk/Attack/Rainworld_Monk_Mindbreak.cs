using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 精神崩溃：造成伤害；目标每有1种负面效果，失去生命。
/// </summary>
public class Rainworld_Monk_Mindbreak : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new("Special", 3m)
    ];

    public Rainworld_Monk_Mindbreak() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var target = cardPlay.Target;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (!target.IsAlive)
            return;

        // 「种」按负面能力的类型去重，同一种负面效果多层只算1种。
        int debuffTypes = target.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .Select(p => p.GetType())
            .Distinct()
            .Count();
        if (debuffTypes <= 0)
            return;

        decimal hpLoss = DynamicVars["Special"].BaseValue * debuffTypes;
        await CreatureCmd.Damage(choiceContext, target,
            new DamageVar(hpLoss, ValueProp.Unblockable | ValueProp.Unpowered), Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
