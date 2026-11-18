using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 情绪吸收：令目标失去所有力量，你失去等量力量。
/// </summary>
public class Rainworld_Monk_Absorb : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<StrengthPower>()
    };

    public Rainworld_Monk_Absorb() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var player = cardPlay.Player.Creature;
        var target = cardPlay.Target;

        // 目标已有的正向力量才会被吸收，负力量视为没有可失去的力量
        var strengthBefore = Math.Max(0, target.GetPowerAmount<StrengthPower>());
        if (strengthBefore <= 0)
            return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, target, -strengthBefore, player, this);

        // 按实际失去的量结算（可能被神器等效果抵消），再让玩家失去等量力量
        var strengthLost = strengthBefore - Math.Max(0, target.GetPowerAmount<StrengthPower>());
        if (strengthLost <= 0)
            return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, player, -strengthLost, player, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
