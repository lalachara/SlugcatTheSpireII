using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Kick : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<TunePower>() };

    private static decimal CalculateDamageMultiplier(CardModel card, Creature? creature)
    {
        if (card is not Rainworld_Monk_Kick)
            return 0m;
        if (card.Owner.Creature.HasPower<TunePower>())
            return card.Owner.Creature.GetPower<TunePower>().Amount;
        return 0m;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateDamageMultiplier)
    ];

    public Rainworld_Monk_Kick() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 有未眩晕的伙伴就多打一次；伤害本身由 CalculatedDamageVar 按「协调」层数加算
        var hits = FriendPositions.HasAwakeFriend(Owner) ? 2 : 1;
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .WithHitCount(hits)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
    }
}