using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 饲养：选择1张手牌消耗，获得协调。消耗。
/// </summary>
public class Rainworld_Monk_Feed : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<TunePower>() };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_Monk_Feed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 塔1 Monk_Feed：选1张手牌消耗（手牌为空则跳过），然后获得协调；只剩1张时直接生效
        var candidates = PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).ToList();
        if (candidates.Count == 1)
        {
            await CardCmd.Exhaust(choiceContext, candidates[0]);
        }
        else if (candidates.Count > 1)
        {
            var selected = await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
                context: choiceContext, player: Owner, filter: c => c != this, source: this);
            foreach (CardModel card in selected)
                await CardCmd.Exhaust(choiceContext, card);
        }

        await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature,
            DynamicVars["Special"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
