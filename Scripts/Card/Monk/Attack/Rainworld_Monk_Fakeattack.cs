using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Fakeattack : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new("Special", 4m),
        // 描述中 {Energy:energyIcons()} 占位：每点被减少的伤害提供 1 点能量。
        new EnergyVar(1)
    ];

    public Rainworld_Monk_Fakeattack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var attack = await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        // 这一击若没能真正打掉目标 Special 点血，差额就转化为能量。
        // UnblockedDamage 是扣完格挡后实际掉的血（不含溢出），所以被格挡吸收的部分同样算作伤害被减少。
        var dealt = attack.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        var expected = DynamicVars["Special"].BaseValue;
        if (dealt < expected)
            await PlayerCmd.GainEnergy((expected - dealt)/2, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Special"].UpgradeValueBy(2m);
    }
}
