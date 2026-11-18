using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 添彩：丢弃任意张手牌，获得等量颜色各不相同的牌。
/// 颜色优先级：其它蛞蝓猫颜色 → 原版角色颜色 → 自身颜色 → 无色。
/// 全部颜色加无色最多 9 种，所以至多只需要丢弃 9 张。
/// </summary>
public class Rainworld_Monk_Addcolor : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>原版手牌上限为 10，最多需要 9 种颜色，因此写死至多丢弃 9 张。</summary>
    private const int maxDiscard = 9;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Colorful };

    /// <summary>手里只剩下这张牌时没有牌可以丢弃。</summary>
    protected override bool IsPlayable => Owner != null && PileType.Hand.GetPile(Owner).Cards.Count > 1;

    public Rainworld_Monk_Addcolor() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var discards = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, maxDiscard), null, this)).ToList();
        if (discards.Count == 0)
            return;

        await CardCmd.Discard(choiceContext, discards);
        await AddColorCards(discards.Count);
    }

    /// <summary>按颜色优先级依次取牌，每张牌的颜色都不相同。</summary>
    private async Task AddColorCards(int amount)
    {
        var selfPoolType = Owner.Character.CardPool.GetType();
        foreach (var pool in Tools.GetColorPoolsInPriorityOrder(selfPoolType))
        {
            if (amount <= 0)
                break;

            var card = Tools.GetRandomCardFromPool(Owner, pool, IsUpgraded);
            if (card == null)
                continue; // 这个颜色没有符合条件的牌，换成下一个颜色

            var targetPile = PileType.Hand.GetPile(Owner).Cards.Count < 10 ? PileType.Hand : PileType.Discard;
            await CardPileCmd.AddGeneratedCardToCombat(card, targetPile, Owner);
            amount--;
        }
    }
}
