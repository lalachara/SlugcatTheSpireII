﻿// Rainworld_River_Wash.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Wash : RiverCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<CorrodePower>() };
    public Rainworld_River_Wash() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
            await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

        var owner = Owner.Creature;
        if (owner.HasPower<WeakPower>())
        {
            var amt = owner.GetPower<WeakPower>()!.Amount;
            await PowerCmd.Remove(owner.GetPower<WeakPower>()!);
            await PowerCmd.Apply<WeakPower>(choiceContext, CombatState!.HittableEnemies, amt, owner, this);
        }

        if (owner.HasPower<CorrodePower>())
        {
            var amt = owner.GetPower<CorrodePower>()!.Amount;
            await PowerCmd.Remove(owner.GetPower<CorrodePower>()!);
            await PowerCmd.Apply<CorrodePower>(choiceContext, CombatState!.HittableEnemies, amt, owner, this);
        }

        if (owner.HasPower<VulnerablePower>())
        {
            var amt = owner.GetPower<VulnerablePower>()!.Amount;
            await PowerCmd.Remove(owner.GetPower<VulnerablePower>()!);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState!.HittableEnemies, amt, owner, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}