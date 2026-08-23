// Rainworld_River_Moisturizer.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Moisturizer : RiverCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_River_Moisturizer() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.HasPower<AdroitcapPower>())
        {
            if (Owner.Creature.GetPower<AdroitPower>() == null)
            {
                await PowerCmd.Apply<AdroitPower>(choiceContext,Owner.Creature,Owner.Creature.GetPower<AdroitcapPower>()!.Amount,Owner.Creature,this);

            }
            else if(Owner.Creature.GetPower<AdroitcapPower>()!.Amount>Owner.Creature.GetPower<AdroitPower>()!.Amount)
            {
                await PowerCmd.Apply<AdroitPower>(choiceContext,Owner.Creature,Owner.Creature.GetPower<AdroitcapPower>()!.Amount-Owner.Creature.GetPower<AdroitPower>()!.Amount,Owner.Creature,this);

            }

            await PowerCmd.Apply<AdroitPower>(choiceContext,Owner.Creature,Owner.Creature.GetPower<AdroitcapPower>()!.Amount-Owner.Creature.GetPower<AdroitPower>()!.Amount,Owner.Creature,this);
        }

        await PowerCmd.Apply<MoisturizerPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}