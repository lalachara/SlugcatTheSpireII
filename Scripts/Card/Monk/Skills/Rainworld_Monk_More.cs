using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_More : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Monk_More() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawnPools = new HashSet<CardPoolModel>();

        while (PileType.Draw.GetPile(Owner).Cards.Any()&&Owner.PlayerCombatState!.Hand.Cards.Count<10)
        {
            var drawn = await CardPileCmd.Draw(choiceContext, 1, Owner);
            var pool = drawn.First().VisualCardPool;
            if (drawnPools.Contains(pool)&&!cardPlay.Card.Keywords.Contains(RainworldKeywords.Colorful))
                break;
            drawnPools.Add(pool);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}