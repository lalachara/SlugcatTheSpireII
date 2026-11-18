using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Linq;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Feel : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 3m)
    ];

    public Rainworld_Monk_Feel() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await CardPileCmd.Draw(choiceContext, (int)DynamicVars["Special"].BaseValue, Owner);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,-1, Owner.Creature, this);
    }



    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}