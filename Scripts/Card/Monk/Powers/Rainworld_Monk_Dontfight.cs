using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Dontfight : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<PloughsharePower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    };

    public Rainworld_Monk_Dontfight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PloughsharePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        // 已经背着负力量时，立刻把这份负力量也复制给所有敌人
        var strength = Owner.Creature.GetPowerAmount<StrengthPower>();
        if (strength >= 0)
            return;

        var enemies = CombatState?.HittableEnemies;
        if (enemies == null)
            return;

        foreach (var enemy in enemies.ToList())
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, strength, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
