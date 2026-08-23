using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Flood : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<CorrodePower>() };
    public Rainworld_River_Flood() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (cardPlay.Target.HasPower<CorrodePower>())
        {
            var cp = cardPlay.Target.GetPower<CorrodePower>()!;
            int amt = cp.Amount;
            await PowerCmd.Apply<CorrodePower>(choiceContext, cardPlay.Target, amt, Owner.Creature, this);

            if (cardPlay.Target.HasPower<CorrodePower>() &&
                cardPlay.Target.GetPower<CorrodePower>()!.Amount >= cardPlay.Target.CurrentHp)
                if (!cardPlay.Target.IsDead)
                    await CreatureCmd.Kill(cardPlay.Target);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}