using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Twistfate:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 0;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new [] { CardKeyword.Exhaust ,RainworldKeywords.Because,RainworldKeywords.Worklevel};



    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];

    public Rainworld_Liver_Twistfate() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel allCard in base.Owner.PlayerCombatState.AllCards)
        {
            if (allCard != this && allCard.IsUpgradable)
            {
                CardCmd.Upgrade(allCard);
            }
        }

        if (Owner.Character is Slugcat)
        {
            SlugcatField.GetSlugCatDataByCreature(Owner.Creature).setworklevel(9);;
        }
        await PowerCmd.Apply<BecausePower>(choiceContext,Owner.Creature,1, base.Owner.Creature, this);


    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}