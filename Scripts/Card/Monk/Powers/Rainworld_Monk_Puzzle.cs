using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 炫彩拼图：获得 1 层"炫彩拼图"能力（每回合你首次打出一种颜色的牌时，抽 1 张牌）。
/// 该能力不叠层，已经有了就不再重复挂。
/// </summary>
public class Rainworld_Monk_Puzzle : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public Rainworld_Monk_Puzzle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1：PuzzlePower 不叠层，已经有了就不重复挂
        if (Owner.Creature.GetPowerAmount<PuzzlePower>() > 0)
            return;
        await PowerCmd.Apply<PuzzlePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级只减 1 费
        EnergyCost.UpgradeBy(-1);
    }
}
