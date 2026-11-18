using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Refusefight : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move)
    ];

    public Rainworld_Monk_Refusefight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}