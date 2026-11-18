using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 心领神会：查看 1 名盟友的整副卡组，从中选 1 张，把它的复制品加入手牌；
/// 本卡强化后，复制品也会被强化。
/// </summary>
public class Rainworld_Monk_Imitate : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Rainworld_Monk_Imitate() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Player? ally = cardPlay.Target.Player;
        if (ally == null || ally == Owner)
            return;

        // 盟友的「卡组」= 战斗中四个牌堆里属于卡组的牌（DeckVersion 非空，排除临时生成的牌）
        var deck = CardPile.GetCards(ally, PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust)
            .Where(c => c.DeckVersion != null)
            .ToList();
        if (deck.Count == 0)
            return;

        // 由自己来挑：网格里显示的是盟友的卡组
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            deck,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (selected == null)
            return;

        // 用选中的这张牌（战斗中的可变实例）克隆：CombatState.CreateCard 只接受「范本」模型，
        // 传可变实例会抛 MutableModelException（之前深度记忆卡死的直接原因）。
        CardModel clone = selected.CreateCloneForPlayer(Owner);

        // 本卡是强化版时，复制品再强化一次（原型已强化 / 不可强化的跳过）
        if (IsUpgraded && clone.IsUpgradable && !clone.IsUpgraded)
        {
            clone.UpgradeInternal();
            clone.FinalizeUpgradeInternal();
        }

        await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
