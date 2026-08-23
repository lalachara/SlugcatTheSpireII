using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_Austerity : FutureCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Ethereal,CardKeyword.Innate };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Future_Austerity() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }

    public override async Task AfterCombatVictoryEarly(CombatRoom room)
    {
        if(this.Pile?.Type==PileType.Exhaust||Owner.Character is not Slugcat)
            return;
        await SlugcatData.addMaxWorkLevel(Owner,1);
        await base.AfterCombatVictoryEarly(room);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Innate);
    }
}