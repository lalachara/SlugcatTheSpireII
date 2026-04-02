using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Eatwhat:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 0;
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
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Food};



    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4m, ValueProp.Move),
        new DynamicVar("Food", 2m),

    ];

    public Rainworld_Liver_Eatwhat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        if (Owner.Character is Slugcat)
        {
            Scripts.SlugcatField.GetSlugCatDataByCreature(Owner.Creature).addfood((int)DynamicVars["Food"].BaseValue);
        }

        await CardPileCmd.ShuffleIfNecessary(choiceContext, base.Owner);
        CardModel cardModel = PileType.Draw.GetPile(base.Owner).Cards.FirstOrDefault();
        if (cardModel != null)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m); 
        DynamicVars["Food"].UpgradeValueBy(1m); 

    }
}