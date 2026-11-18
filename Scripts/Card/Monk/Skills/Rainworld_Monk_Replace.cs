using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 置换反应：令目标获得固定格挡，并失去等量生命。
/// 本回合结束时，其失去所有格挡并回复等量生命（由 ReplacePower 结算）。
/// </summary>
/// <remarks>
/// 格挡固定、不走任何修正：塔2 的 MultiplayerScalingModel 会把「目标为敌方、且带 Move 的格挡」
/// 按玩家人数放大，一旦放大就不再是等量交换——敌方多拿的格挡会在回合末换成等量生命，
/// 等于反给敌人回血。所以这里用 Unpowered，顺带也不吃敏捷 / 脆弱。
/// </remarks>
public class Rainworld_Monk_Replace : MonkCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Unpowered)
    ];

    public Rainworld_Monk_Replace() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var amount = DynamicVars.Block.BaseValue;
        // 先给目标格挡、再让它失去等量生命：这一步是纯粹的等量交换，
        // 失血不吃格挡、也不走攻击结算，所以不会被刚发出去的这份格挡抵消
        await CreatureCmd.GainBlock(cardPlay.Target, amount, ValueProp.Unpowered, cardPlay);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, amount,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
        // 回合结束时把目标手里的格挡全部换成等量生命，交给 ReplacePower 在回合末结算
        await PowerCmd.Apply<ReplacePower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}
