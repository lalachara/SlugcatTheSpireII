using System;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Spring : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;

    private readonly decimal _initialDamage = 8m;
    private readonly decimal _initialBlock = 0m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(_initialDamage, ValueProp.Move),
        new BlockVar(_initialBlock, ValueProp.Move),
        new("Special", 2m)
    ];

    public Rainworld_Monk_Spring() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            DynamicVars.Damage.BaseValue = _initialDamage;
            DynamicVars.Block.BaseValue = _initialBlock;
        }
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner)
        {
            DynamicVars.Damage.BaseValue = Math.Max(0m, DynamicVars.Damage.BaseValue - DynamicVars["Special"].BaseValue);
            DynamicVars.Block.BaseValue += DynamicVars.Block.BaseValue;
        }
        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}