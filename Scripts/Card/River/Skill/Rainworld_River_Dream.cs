// Rainworld_River_Dream.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Dream : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>() ,HoverTipFactory.FromPower<AdroitcapPower>() };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m)
    ];

    public Rainworld_River_Dream() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AdroitcapPower>(choiceContext, Owner.Creature, DynamicVars["Special"].BaseValue,
            Owner.Creature, this);
        EnergyCost.UpgradeBy(1);
        DynamicVars["Special"].UpgradeValueBy(1);

    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}