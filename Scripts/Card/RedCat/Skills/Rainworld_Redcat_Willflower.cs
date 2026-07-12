using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Willflower : RedCatCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Workreport),HoverTipFactory.FromCard<Wound>(), };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)

    ];

    public Rainworld_Redcat_Willflower() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public void AddMisc(int amount)
    {
        DynamicVars.Energy.BaseValue += 1;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            CombatState.CreateCard<Wound>(Owner), PileType.Draw, Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}