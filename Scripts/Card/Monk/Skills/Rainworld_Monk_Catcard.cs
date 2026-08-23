using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Scripts.Card.Future;
using Rainworld.Scripts.Card.Liver;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Card.River;
using System.Collections.Generic;
using System.Linq;
using Rainworld.Resource.Card;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Catcard : MonkCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;


    //public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Monk_Catcard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var poolCards = ModelDb.CardPool<Rainworld_Liver_CardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Concat(ModelDb.CardPool<Rainworld_Redcat_CardPool>()
                .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Concat(ModelDb.CardPool<Rainworld_River_CardPool>()
                .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c =>( c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Power) && (c.Rarity==CardRarity.Common || c.Rarity==CardRarity.Uncommon || c.Rarity==CardRarity.Rare))
            .ToList();

        if (poolCards.Count == 0)
            return;

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var selected = poolCards.OrderBy(_ => rng.NextInt(0, int.MaxValue)).Take(3).ToList();
        var options = CardFactory.GetDistinctForCombat(Owner, selected, selected.Count, rng).ToList();

        if (IsUpgraded)
            foreach (var card in options)
                CardCmd.Upgrade(card);

        var chosen = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(), options, Owner, canSkip: true);

        if (chosen != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);

        }
    }


}