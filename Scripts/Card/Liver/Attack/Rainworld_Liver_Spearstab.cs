using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Spearstab:LiverCardModelAtk

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Spear,RainworldKeywords.Chuang};


    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Chuang",2m)
    ];

    public Rainworld_Liver_Spearstab() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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

        decimal temp = DynamicVars.Damage.BaseValue;
        foreach (Creature m in base.CombatState.HittableEnemies)
        {
            if (m != cardPlay.Target)
            {
                temp -= 2;
                if (temp < 0)
                    return;
                
                await DamageCmd
                        .Attack(temp)
                        .FromCard(this)
                        .Targeting(m)
                        .WithHitFx("vfx/vfx_attack_slash")
                        .Execute(choiceContext);   
                await PowerCmd.Apply<ChuangPower>(choiceContext,m, DynamicVars["Chuang"].BaseValue, base.Owner.Creature, this);

                
            }

            
        }

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m); // 升级后增加4点伤害
        DynamicVars["Chuang"].UpgradeValueBy(1m);
    }
}