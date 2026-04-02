using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Neurone:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 0;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Ethereal,CardKeyword.Exhaust,RainworldKeywords.Worklevel,RainworldKeywords.Food};



    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1)
    ];

    public Rainworld_Liver_Neurone() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainMaxHp(Owner.Creature,DynamicVars.Cards.BaseValue+10000m);
        if(Owner.Character is Slugcat)
            SlugcatField.GetSlugCatDataByCreature(Owner.Creature).addfood(1);
        if(Owner.Deck.Cards.Contains(base.DeckVersion))
            await CardPileCmd.RemoveFromDeck(base.DeckVersion);
        
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m); 
    }
}