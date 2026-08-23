using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Imitate : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Monk_Imitate() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var allyPlayer = cardPlay.Target.Player;
        if (allyPlayer == null)
            return;

        var allyPowers = cardPlay.Target.Powers
            .Where(p => p.Type == PowerType.Buff)
            .ToList();
        if (allyPowers.Count > 0)
        {
            var randomPower = allyPowers[Owner.RunState.Rng.CombatCardGeneration.NextInt(0, allyPowers.Count)];
            var amount = randomPower.Amount;
            var powerType = randomPower.GetType();
            var method = typeof(PowerCmd).GetMethod("Apply")!.MakeGenericMethod(powerType);
            await (Task)method.Invoke(null, new object[] { choiceContext, Owner.Creature, amount, Owner.Creature, this })!;
        }

        var handCards = PileType.Hand.GetPile(allyPlayer).Cards.ToList();
        if (handCards.Count > 0)
        {
            var maxRarity = handCards.Max(c => c.Rarity);
            var rareCards = handCards.Where(c => c.Rarity == maxRarity).ToList();
            var picked = rareCards[Owner.RunState.Rng.CombatCardGeneration.NextInt(0, rareCards.Count)];
            var distinct = CardFactory.GetDistinctForCombat(Owner, new[] { picked }, 1, Owner.RunState.Rng.CombatCardGeneration);
            var clone = distinct.First();
            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}