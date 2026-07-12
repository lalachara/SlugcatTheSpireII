// Rainworld_River_Eddy.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Eddy : RiverCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override bool HasEnergyCostX => true;  // 加这行
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<CorrodePower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new("Special", 2m)
    ];

    public Rainworld_River_Eddy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var times = ResolveEnergyXValue();
        for (var i = 0; i < times; i++)
            foreach (var enemy in CombatState.HittableEnemies)
            {
                await DamageCmd
                    .Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this,cardPlay)
                    .Targeting(enemy)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                await PowerCmd.Apply<CorrodePower>(choiceContext, enemy, DynamicVars["Special"].BaseValue,
                    Owner.Creature, this);
            }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}