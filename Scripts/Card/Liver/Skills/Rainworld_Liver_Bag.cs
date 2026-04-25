using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Bag:LiverCardModel

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
    public override int MaxUpgradeLevel => 0;
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust,RainworldKeywords.Treasure};
    public List<CardModel> Cards = new List<CardModel>();
    
    
    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        
    ];

    public Rainworld_Liver_Bag() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public Rainworld_Liver_Bag(List<CardModel> cards,Player owner) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        Cards = cards;
        this.Owner = owner;
    }
    
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Cards.Count == 0)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(new Void() , PileType.Hand,Owner));
            
        }

        else
        {
            foreach (var card in Cards)
            {
                card.Owner = Owner;
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card , PileType.Hand, Owner));

            }

        }
        Cards = new List<CardModel>();


    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}