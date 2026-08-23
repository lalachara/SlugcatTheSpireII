using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Know : MonkCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust,CardKeyword.Ethereal };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.SummonStatic), HoverTipFactory.Static(StaticHoverTip.Forge), HoverTipFactory.FromOrb<LightningOrb>() };
    public Rainworld_Monk_Know() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
        await OstyCmd.Summon(choiceContext, Owner, 5, this);
        await ForgeCmd.Forge(5, Owner, this);
        await OrbCmd.Channel<LightningOrb>(choiceContext, Owner);
        await CardPileCmd.Draw(choiceContext, 2, Owner);
        var list = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 2, 2), null, this)).ToList();
        await CardCmd.Discard(choiceContext, list);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}