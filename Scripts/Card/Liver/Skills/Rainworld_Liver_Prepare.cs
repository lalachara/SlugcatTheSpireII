using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Liver;

public class Rainworld_Liver_Prepare:LiverCardModel

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
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust};

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(0),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedEnergy").WithMultiplier(CalculateDeckEnergyMultiplier)    ];
    private static decimal CalculateDeckEnergyMultiplier(CardModel card, Creature? _)
    {
        // 从 card 获取 Owner，不访问实例
        var owner = card.Owner;
        if (owner == null) return 0m;

        return PileType.Deck.GetPile(owner).Cards.Count() / 8m;
    }
    public Rainworld_Liver_Prepare() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(((CalculatedVar)base.DynamicVars["CalculatedEnergy"]).Calculate(Owner.Creature), base.Owner);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);

        
    }
}