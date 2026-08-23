using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Windrain : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    public Rainworld_Monk_Windrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var strikes = PileType.Draw.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) && c is not Rainworld_Monk_Windrain)
            .ToList();
        foreach (var strike in strikes)
        {
            await CardCmd.AutoPlay(choiceContext, strike, cardPlay.Target);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}