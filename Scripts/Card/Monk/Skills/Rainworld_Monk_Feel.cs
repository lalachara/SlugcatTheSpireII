using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Linq;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Feel : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 3m)
    ];

    public Rainworld_Monk_Feel() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        bool ex = PileType.Hand.GetPile(Owner).Cards
            .Select(c => c.VisualCardPool)
            .Distinct()
            .Count() >= 3;
        await CardPileCmd.Draw(choiceContext, (int)DynamicVars["Special"].BaseValue+(ex?1:0), Owner);
    }



    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}