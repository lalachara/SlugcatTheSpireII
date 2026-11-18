using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 精神附魔：选择1名盟友，本回合其造成伤害时，你对目标额外造成体力流失。
/// </summary>
public class Rainworld_Monk_Mindmagic : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 4m)
    ];

    public Rainworld_Monk_Mindmagic() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (!cardPlay.Target.IsAlive)
            return;

        await PowerCmd.Apply<MindMagicPower>(choiceContext, cardPlay.Target,
            DynamicVars["Special"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(2m);
    }
}
