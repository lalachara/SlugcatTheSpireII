using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Open : RedCatCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 30m)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Corrupt,CardKeyword.Exhaust
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
        { HoverTipFactory.FromPower<MushblockPower>(),HoverTipFactory.FromPower<RedmushroomPower>() };

    public Rainworld_Redcat_Open() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MushblockPower>(choiceContext, Owner.Creature, (int)DynamicVars["Special"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(10m);
    }
}