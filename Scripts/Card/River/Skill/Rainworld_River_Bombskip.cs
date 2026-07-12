// Rainworld_River_BombSkip.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Bombskip : RiverCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>() };
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m),
        new EnergyVar(1)
    ];

    public Rainworld_River_Bombskip() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override bool IsPlayable =>
        Owner.Creature.HasPower<AdroitPower>() && Owner.Creature.GetPower<AdroitPower>()!.Amount > 0;


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.HasPower<AdroitPower>() && Owner.Creature.GetPower<AdroitPower>()!.Amount > 0)
        {
            await PowerCmd.ModifyAmount(choiceContext, Owner.Creature.GetPower<AdroitPower>()!, -1, Owner.Creature,
                this);
            await CardPileCmd.Draw(choiceContext, 2, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}