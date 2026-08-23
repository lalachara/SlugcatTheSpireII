using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Stopfight : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.FromPower<StopfightPower>()
    };

    public Rainworld_Monk_Stopfight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(Owner.Creature.HasPower<StopfightPower>())
            await PowerCmd.ModifyAmount(choiceContext,Owner.Creature.GetPower<StopfightPower>(),1,Owner.Creature,this);
        else
        {
            await PowerCmd.Apply<StopfightPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);
        }
        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);


    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}