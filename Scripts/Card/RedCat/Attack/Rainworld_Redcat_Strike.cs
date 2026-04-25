using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Strike:RedCatCardModelAtk

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Basic;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike ,Tools.CatStrike_Tag};


    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move),
    ];

    public Rainworld_Redcat_Strike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m); // 升级后增加4点伤害
    }
}