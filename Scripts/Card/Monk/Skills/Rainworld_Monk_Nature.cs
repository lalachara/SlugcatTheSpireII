using System;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Nature : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 每 1 点被重置掉的力量，所有敌人失去 1 份 Special 点生命。
        new("Special", 4m)
    ];

    public Rainworld_Monk_Nature() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var strength = Owner.Creature.GetPowerAmount<StrengthPower>();
        if (strength == 0)
            return;

        // 把力量重置为 0，并按这次变化的点数结算：每 1 点让所有敌人失去 Special 点生命。
        var change = Math.Abs(strength);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -strength, Owner.Creature, this);

        // 生命流失同时带“不可格挡”和“无能力来源”标记，不吃格挡，也不受力量/虚弱修正。
        if (CombatState == null)
            return;
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies,
            change * DynamicVars["Special"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 塔1：升级只提升每点力量对应的生命流失（4 → 6），虚无保留。
        DynamicVars["Special"].UpgradeValueBy(2m);
    }
}
