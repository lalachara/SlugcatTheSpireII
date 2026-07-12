// Rainworld_River_WaterDefend.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Waterdefend : RiverCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_River_Waterdefend() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amt = (int)DynamicVars["Special"].BaseValue;
        await PowerCmd.Apply<BufferPower>(choiceContext, Owner.Creature, amt, Owner.Creature, this);

        if (Owner.Creature.HasPower<BufferPower>())
            await CardPileCmd.Draw(choiceContext, Owner.Creature.GetPower<BufferPower>()!.Amount, Owner);
        else
            await CardPileCmd.Draw(choiceContext, amt, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}