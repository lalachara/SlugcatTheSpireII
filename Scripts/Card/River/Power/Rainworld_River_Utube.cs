using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Keywords;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Utube : RiverCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_River_Utube() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null && cardPlay.Target != Owner.Creature)
        {
            var targetpower =   await PowerCmd.Apply<UtubePower>(choiceContext, cardPlay.Target, 1, Owner.Creature, null);
            targetpower!.target = Owner.Creature; 
            var mypower = await PowerCmd.Apply<UtubePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            mypower!.target = cardPlay.Target;
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}