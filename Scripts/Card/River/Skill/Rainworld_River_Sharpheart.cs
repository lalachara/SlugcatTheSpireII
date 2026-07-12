// Rainworld_River_SharpHeart.cs

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Sharpheart : RiverCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>() };
    public Rainworld_River_Sharpheart() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile.Cards, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, 1))).FirstOrDefault();
            if (selected != null)
                await CardPileCmd.Add(selected, PileType.Hand);
        }

        var handPile = PileType.Hand.GetPile(Owner);
        if (handPile.Cards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
                context: choiceContext, player: Owner, filter: null, source: this)).FirstOrDefault();
            if (selected != null)
                await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
            
        }

        if (Owner.Creature.HasPower<AdroitPower>() && IsUpgraded)
            if (Owner.Creature.GetPower<AdroitPower>()!.Amount > 0)
                return;
        AddKeyword(CardKeyword.Exhaust);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}