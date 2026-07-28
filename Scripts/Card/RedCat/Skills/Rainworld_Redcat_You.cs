using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using System.Linq;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_You : RedCatCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m)
    ];

    public Rainworld_Redcat_You() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var allAttackCards = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Hand.GetPile(Owner).Cards)
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Where(c => c.Type == CardType.Attack)
            .Distinct()
            .ToList();

        if (allAttackCards.Count == 0)
            return;

        var priorityCards = allAttackCards
            .Where(c => !c.Keywords.Contains(CardKeyword.Exhaust) && c.Rarity != CardRarity.Basic)
            .ToList();

        var count = (int)DynamicVars["Special"].BaseValue;
        var selected = new List<CardModel>();

        while (selected.Count < count && priorityCards.Count > 0)
        {
            var card = Owner.RunState.Rng.Shuffle.NextItem(priorityCards);
            selected.Add(card);
            priorityCards.Remove(card);
            allAttackCards.Remove(card);
        }

        while (selected.Count < count && allAttackCards.Count > 0)
        {
            var card = Owner.RunState.Rng.Shuffle.NextItem(allAttackCards);
            selected.Add(card);
            allAttackCards.Remove(card);
        }

        foreach (var card in selected)
        {
            var clone = card.CreateCloneForPlayer(cardPlay.Target!.Player!);
            clone.EnergyCost.SetThisCombat(0);
            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Draw, cardPlay.Target!.Player!);
        }
    }
    
    

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}