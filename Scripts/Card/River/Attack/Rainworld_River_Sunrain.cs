﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Sunrain : RiverCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, ValueProp.Move),
        new CardsVar(6),
        new EnergyVar(1)
    ];

    public Rainworld_River_Sunrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

                await DamageCmd
                    .Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .TargetingAllOpponents(CombatState!)
                    .WithHitCount(2)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);

        await CreatureCmd.Heal(Owner.Creature, 6, true);
        await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}