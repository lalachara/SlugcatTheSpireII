using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 空想具现：选择1张手牌，将其复制品加入你的卡组。消耗。
/// </summary>
public class Rainworld_Monk_Projection : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;


    public Rainworld_Monk_Projection() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 排除正在打出的这张牌本身，以及不应被复制的任务牌。
        var candidates = PileType.Hand.GetPile(Owner).Cards.Where(CanCopy).ToList();
        if (candidates.Count == 0)
            return;

        CardModel? selected;
        if (candidates.Count == 1)
        {
            selected = candidates[0];
        }
        else
        {
            selected = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
                context: choiceContext, player: Owner, filter: CanCopy, source: this)).FirstOrDefault();
        }

        if (selected == null)
            return;

        // 手牌是战斗副本；优先复制对应的卡组本体，避免把战斗中的临时状态带进卡组。
        CardModel source = selected.DeckVersion is { } deckVersion && Owner.Deck.Cards.Contains(deckVersion)
            ? deckVersion
            : selected;
        CardModel copy = Owner.RunState.CloneCard(source);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(copy, PileType.Deck));
    }

    private bool CanCopy(CardModel card)
        => true;

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
