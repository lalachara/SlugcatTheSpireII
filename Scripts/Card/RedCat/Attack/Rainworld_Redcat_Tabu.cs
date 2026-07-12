using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Tabu : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Corrupt
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Corrupt),
        HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>(),HoverTipFactory.FromCard<Rainworld_Redcat_Corrupt>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
        new("Summon", 1m)
    ];

    public Rainworld_Redcat_Tabu() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hitCount = (int)DynamicVars["Summon"].BaseValue;
        var corruptExhausted = Owner.PlayerCombatState.ExhaustPile.Cards
            .Count(c => c.Keywords.Contains(RainworldKeywords.Corrupt));
        hitCount += corruptExhausted;
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithHitCount(hitCount)
            .Execute(choiceContext);
        await CardPileCmd.AddGeneratedCardToCombat(
            CombatState.CreateCard<Rainworld_Redcat_Corrupt>(Owner), PileType.Hand, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            CombatState.CreateCard<Rainworld_Redcat_Corrupt>(Owner), PileType.Hand, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            CombatState.CreateCard<Rainworld_Redcat_Corrupt>(Owner), PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}