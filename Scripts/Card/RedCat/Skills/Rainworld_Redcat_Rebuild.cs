using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Rebuild : RedCatCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Redcat_Rebuild() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Corrupt), };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var exhaustPile = PileType.Exhaust.GetPile(Owner);
        var exhaustCards = exhaustPile.Cards
            .ToList();
        if (exhaustCards.Count == 0) return;
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, exhaustCards, Owner, prefs)).FirstOrDefault();
        if (selected != null)
        {
            if (!selected.Keywords.Contains(RainworldKeywords.Corrupt)&&!selected.Keywords.Contains(RainworldKeywords.Cantcorrupt)&&!selected.Keywords.Contains(RainworldKeywords.Immunity))
                await Tools.CorruptCard(Owner, selected);
            await CardPileCmd.Add(selected, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); 
    }
}