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
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Help : LiverCardModel

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

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromCard<Rainworld_Liver_Birdhelp>(IsUpgraded),
        HoverTipFactory.FromCard<Rainworld_Liver_Cabhelp>(IsUpgraded),
        HoverTipFactory.FromCard<Rainworld_Liver_Monkeyhelp>(IsUpgraded)
    };


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Help() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel;

        var cards = new List<CardModel>
        {
            CombatState.CreateCard<Rainworld_Liver_Monkeyhelp>(Owner),
            CombatState.CreateCard<Rainworld_Liver_Cabhelp>(Owner),
            CombatState.CreateCard<Rainworld_Liver_Birdhelp>(Owner)
        };
        if (IsUpgraded)
            foreach (var card in cards)
                CardCmd.Upgrade(card);
        cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner, false);

        if (cardModel != null) await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, Owner);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}