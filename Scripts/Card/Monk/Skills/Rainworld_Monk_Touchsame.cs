using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 共感：获得格挡，令目标失去等量生命。
/// </summary>
public class Rainworld_Monk_Touchsame : MonkCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override bool HasEnergyCostX => true;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move)
    ];

    public Rainworld_Monk_Touchsame() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // X 费：引擎已按卡面自动扣掉支付的能量，这里取实际支付的 X（含化学X 等 X 修正）；
        // 塔1 TouchSameAction 就是循环 X 次：每次获得一份格挡，再令目标失去等量生命
        int times = ResolveEnergyXValue();
        for (int i = 0; i < times; i++)
        {
            // 目标中途被失去生命打死就不再继续结算（塔1 的失血动作同样不会再命中尸体）
            if (!cardPlay.Target.IsAlive)
                break;
            // 现取实际获得的格挡（含敏捷、脆弱等修正），
            // 保证"获得多少格挡"和"让目标失去多少生命"必然是同一个数
            var gained = await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            // 失血不吃格挡，所以刚给自己的这份格挡不会把失血抵消掉
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, gained,
                ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
