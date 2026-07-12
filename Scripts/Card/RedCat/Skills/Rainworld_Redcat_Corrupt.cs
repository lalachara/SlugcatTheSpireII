using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Corrupt : RedCatCardModel

{
    private const int energyCost = 0;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<RedmushroomPower>(),
        HoverTipFactory.FromPower<MushblockPower>(),
    };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Corrupt, CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(2m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Mushblock").WithMultiplier((CardModel card, Creature? _) =>
            card.Owner.Creature?.GetPower<SubstratePower>()?.Amount ?? 0)
    ];

    public Rainworld_Redcat_Corrupt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override int MaxUpgradeLevel { get; } = 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mushblock = (int)((CalculatedVar)DynamicVars["Mushblock"]).Calculate(null);
        await PowerCmd.Apply<MushblockPower>(choiceContext, Owner.Creature, mushblock, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}