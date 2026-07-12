using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Circulation : RedCatCardModel

{
    // 基础耗能
    private const int energyCost = -1;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    protected override bool HasEnergyCostX => true;  // 加这行

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust,RainworldKeywords.Food};

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public Rainworld_Redcat_Circulation() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        await CardPileCmd.Draw(choiceContext, x, Owner);
        var list = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, IsUpgraded ? 999999999 : x), null, this)).ToList();
        if (list.Count != 0)
        {
            await CardCmd.Discard(choiceContext, list);
            await PlayerCmd.GainEnergy(list.Count, Owner);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}