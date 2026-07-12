using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Solidify : RedCatCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Summon", 3m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<VigorPower>(), };
    public Rainworld_Redcat_Solidify() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.HasPower<VigorPower>())
        {
            var vigorAmount = Owner.Creature.GetPowerAmount<VigorPower>();
            var divisor = (int)DynamicVars["Summon"].BaseValue;
            var strength = (vigorAmount + divisor - 1) / divisor;
            await PowerCmd.Remove(Owner.Creature.GetPower<VigorPower>()!);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Summon"].UpgradeValueBy(-1m);
    }
}