// Rainworld_River_Tidalbore.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Tidalbore : RiverCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new EnergyVar(1),
        new("Special", 0m)
    ];

    public Rainworld_River_Tidalbore() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var dmg = DynamicVars.Damage.BaseValue;
        var results = (await DamageCmd
            .Attack(dmg)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)).Results;
        var unblockedDamage = results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (unblockedDamage > DynamicVars["Special"].BaseValue)
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        DynamicVars["Special"].BaseValue = (unblockedDamage);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}