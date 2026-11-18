using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Shamanform : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal};
    
    //protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>()};

    public Rainworld_Monk_Shamanform() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ShamanFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}