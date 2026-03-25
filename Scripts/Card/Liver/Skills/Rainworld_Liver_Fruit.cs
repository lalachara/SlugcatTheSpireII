using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Fruit:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 0;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Token;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> {  };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Retain,CardKeyword.Exhaust,RainworldKeywords.Food};
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Food", 1m),
        new EnergyVar(1)
    ];
    
    public Rainworld_Liver_Fruit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
        if (IsUpgraded)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Liver_Fruit>(base.Owner) , PileType.Hand, addedByPlayer: true));
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Liver_Fruit>(base.Owner) , PileType.Hand, addedByPlayer: true));
        }

        if (Owner.Character is Slugcat)
        {
            Scripts.SlugcatField.GetSlugCatDataByCreature(Owner.Creature).addfood((int)DynamicVars["Food"].BaseValue);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Food"].UpgradeValueBy(1m); 
        
        
    }
}