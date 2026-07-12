using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Wetting : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.AdroitNotUsed) };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_River_Wetting() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = PileType.Hand.GetPile(Owner);

        if (handPile.Cards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
                context: choiceContext, player: Owner, filter: null, source: this)).FirstOrDefault();


            if (selected != null)
            {
                CardCmd.ApplyKeyword(selected, RainworldKeywords.AdroitNotUsed);
                CardCmd.RemoveKeyword(selected, CardKeyword.Exhaust);
            }
        }

    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);

    }
}