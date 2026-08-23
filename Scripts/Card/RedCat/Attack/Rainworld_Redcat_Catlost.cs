using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Catlost : RedCatCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,RainworldKeywords.Backtohand
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new("Special", 1m)
    ];

    public Rainworld_Redcat_Catlost() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    // {
    //     if (player == Owner)
    //     {
    //         var rng = player.RunState.Rng.CombatTargets;
    //         DynamicVars.Damage.BaseValue = rng.NextInt(2, IsUpgraded ? 11 : 10);
    //         DynamicVars["Summon"].BaseValue = rng.NextInt(1, IsUpgraded ? 4 : 3);
    //     }
    //     await base.AfterPlayerTurnStart(choiceContext, player);
    // }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var hitCount = (int)DynamicVars["Special"].BaseValue;
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithHitCount(hitCount)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}