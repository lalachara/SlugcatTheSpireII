using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Eat : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>()};
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [
            RainworldKeywords.Corrupt
        ];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];
    
    protected override bool IsPlayable => Owner.Creature.HasPower<RedmushroomPower>();

    public Rainworld_Redcat_Eat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .WithHitFx(sfx:"event:/Rainworld/sfx/eat")
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
        

        var playerEffectiveHp = Owner.Creature.CurrentHp;
        if (Owner.Creature.HasPower<MushblockPower>())
            playerEffectiveHp += Owner.Creature.GetPowerAmount<MushblockPower>();

        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (!enemy.IsDead && enemy.CurrentHp < playerEffectiveHp)
            {
                await CreatureCmd.Kill(enemy);
                await CreatureCmd.GainMaxHp(Owner.Creature, 5m);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}