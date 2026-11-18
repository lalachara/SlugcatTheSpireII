using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 和平主义：直到你的下个回合结束，你造成和受到的伤害变为1；失去1点力量。消耗。
/// </summary>
public class Rainworld_Monk_Peace : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    public Rainworld_Monk_Peace() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PeacePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        // 塔1：失去 1 点力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 塔1：升级后 2 费 → 1 费
        EnergyCost.UpgradeBy(-1);
    }
}
