using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.CardVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Help:LiverCardModel

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
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> {  };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust};
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new []{HoverTipFactory.FromCard<Rainworld_Liver_Birdhelp>(base.IsUpgraded),HoverTipFactory.FromCard<Rainworld_Liver_Cabhelp>(base.IsUpgraded),HoverTipFactory.FromCard<Rainworld_Liver_Monkeyhelp>(base.IsUpgraded)};

    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    
    public Rainworld_Liver_Help() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel;

            List<CardModel> cards = new List<CardModel>()
            {CombatState.CreateCard<Rainworld_Liver_Monkeyhelp>(base.Owner), CombatState.CreateCard<Rainworld_Liver_Cabhelp>(base.Owner), CombatState.CreateCard<Rainworld_Liver_Birdhelp>(base.Owner) };
            if (IsUpgraded)
            {
                foreach (CardModel card in cards)
                    CardCmd.Upgrade(card);
            }
            cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner, canSkip: false);
            
        if (cardModel != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        
    }
}