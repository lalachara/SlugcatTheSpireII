using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.River.Skills;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Fullback : MonkCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Liver_Fruit>(), HoverTipFactory.FromCard<Rainworld_Redcat_Smallcent>(), HoverTipFactory.FromCard<Rainworld_River_Waterfruit>() };

    public Rainworld_Monk_Fullback() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var player in CombatState.Players)
        {
            if (!player.Creature.IsAlive)
                continue;
            var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 3);
            switch (random)
            {
                case 0:
                    await CardPileCmd.AddGeneratedCardToCombat(
                        CombatState.CreateCard<Rainworld_Liver_Fruit>(player), PileType.Hand, player);
                    break;
                case 1:
                    await CardPileCmd.AddGeneratedCardToCombat(
                        base.CombatState.CreateCard<Rainworld_Redcat_Smallcent>(player), PileType.Hand, player);
                    break;
                case 2:
                    await CardPileCmd.AddGeneratedCardToCombat(
                        base.CombatState.CreateCard<Rainworld_River_Waterfruit>(player), PileType.Hand, player);
                    break;

            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}