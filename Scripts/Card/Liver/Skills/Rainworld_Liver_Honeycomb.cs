using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Honeycomb:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Ethereal,CardKeyword.Exhaust};
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ConstrictPower",15)
    ];
    
    public Rainworld_Liver_Honeycomb() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SlowPower>(choiceContext,cardPlay.Target, 1, base.Owner.Creature, this);
        await PowerCmd.Apply<ConstrictPower>(choiceContext,cardPlay.Target, base.DynamicVars["ConstrictPower"].BaseValue, base.Owner.Creature, this);

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["ConstrictPower"].UpgradeValueBy(5); 
        
    }
}