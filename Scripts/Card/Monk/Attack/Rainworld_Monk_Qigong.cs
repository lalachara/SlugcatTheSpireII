using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Qigong : MonkCardModel
{
    protected override bool HasEnergyCostX => true;
    private const int energyCost = -1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, ValueProp.Move),
        new EnergyVar(1)
    ];

    public Rainworld_Monk_Qigong() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var x = ResolveEnergyXValue();
        var remaining = x;
        Creature? currentTarget = cardPlay.Target;
        while (remaining > 0 && currentTarget != null && currentTarget.IsAlive)
        {
            await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(currentTarget)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            remaining--;
            if (!currentTarget.IsAlive)
            {
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
                currentTarget = CombatState.HittableEnemies
                    .Where(e => e.IsAlive)
                    .OrderBy(e => e.CurrentHp)
                    .FirstOrDefault();
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}