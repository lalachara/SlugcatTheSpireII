using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Mdbaole : LiverCardModel

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
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>(),HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel) };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3m),
        new PowerVar<DexterityPower>(3m)
    ];

    public Rainworld_Liver_Mdbaole() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MDbaole2>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature,
            this);
        await PowerCmd.Apply<MDbaole1>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature,
            this);
        await CardPileCmd.Draw(choiceContext, 3, Owner);
        foreach (var card in PileType.Hand.GetPile(Owner).Cards)
            if (!card.EnergyCost.CostsX)
                card.SetToFreeThisTurn();

        await PowerCmd.Apply<TiredPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}