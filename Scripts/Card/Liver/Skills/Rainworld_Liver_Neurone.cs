using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Neurone : LiverCardModel

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

    // 不可被生成类效果抽到：打出即永久加最大生命并从卡组移除，随机生成会白送资源。
    protected override HashSet<CardTag> CanonicalTags => new() { RainworldCardTags.CantGenerate };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel),HoverTipFactory.FromKeyword(RainworldKeywords.Food) };
    
    public override TargetType TargetType
    {
        get => !this.HasSharedFood ? TargetType.Self : TargetType.AllAllies;
    }
    private bool HasSharedFood
    {
        get => this.IsMutable&& this.Owner!=null  && this.Owner.Creature.HasPower<SharefoodPower>();
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        { CardKeyword.Ethereal, CardKeyword.Exhaust ,RainworldKeywords.Foodcard};


    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public Rainworld_Liver_Neurone() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
        if (HasSharedFood)
        {
            foreach (var player in CombatState!.Players)
            {
                if (player.Character is Slugcat)
                {
                     await CreatureCmd.GainMaxHp(player.Creature, DynamicVars.Cards.BaseValue*5);
                     if (player.Character is Slugcat)
                        await SlugcatData.addfood(player, 1);

                }

               
            }
        }
        else
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.Cards.BaseValue*5);
            if (Owner.Character is Slugcat)
                await SlugcatData.addfood(Owner, 1);
        }
        if (Owner.Deck.Cards.Contains(DeckVersion))
            await CardPileCmd.RemoveFromDeck(DeckVersion!);


    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}