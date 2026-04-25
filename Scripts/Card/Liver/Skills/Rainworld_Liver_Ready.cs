using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Liver;

public class Rainworld_Liver_Ready:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };


    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6m, ValueProp.Move),
        new PowerVar<VigorPower>(4m)
    ];

    public Rainworld_Liver_Ready() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<VigorPower>(choiceContext,base.Owner.Creature, base.DynamicVars["VigorPower"].IntValue, base.Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
        base.DynamicVars["VigorPower"].UpgradeValueBy(1m);
        
    }
}