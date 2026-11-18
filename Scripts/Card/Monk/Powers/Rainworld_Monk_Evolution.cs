using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 进化：伙伴造成的伤害翻倍。能力牌。
/// </summary>
public class Rainworld_Monk_Evolution : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {  HoverTipFactory.FromKeyword(RainworldKeywords.Companion) };

    public Rainworld_Monk_Evolution() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Evolution：每层使伙伴伤害 ×(1+层数)（倍率由伙伴侧 FriendPetBase 读取）
        await PowerCmd.Apply<MonkEvolutionPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
