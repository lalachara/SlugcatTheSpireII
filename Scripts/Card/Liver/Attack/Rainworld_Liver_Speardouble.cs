using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Speardouble:LiverCardModelAtk

{
    // 基础耗能
    private const int energyCost = 2;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Spear,RainworldKeywords.Chuang};

    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust};


    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move),
        new DynamicVar("Chuang", 2m)
    ];

    public Rainworld_Liver_Speardouble() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
        await PowerCmd.Apply<ChuangPower>(choiceContext,cardPlay.Target, DynamicVars["Chuang"].BaseValue, base.Owner.Creature, this);
        await DamageCmd
            .Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<ChuangPower>(choiceContext,cardPlay.Target,DynamicVars["Chuang"].BaseValue, base.Owner.Creature, this);

        await Cmd.Wait(0.5f);
        

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m); // 升级后增加4点伤害
    }
}