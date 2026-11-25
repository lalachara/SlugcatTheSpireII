using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_Insight : FutureCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1)
    ];

    public Rainworld_Future_Insight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel card in Owner.PlayerCombatState!.AllCards)
        {
            CardEnergyCost energyCost = card.EnergyCost;

            // X 费牌没有固定耗能，UpgradeBy 对它们本来就不生效
            if (energyCost.CostsX)
                continue;

            // 用「卡面实际耗能」（含各种修正）判断减费前后的变化：
            // 只有原本 >0、被这次 -1 降到 0 的非能力牌才获得消耗；
            // 本来就是 0 费的牌维持原样
            int costBefore = energyCost.GetWithModifiers(CostModifiers.All);
            energyCost.UpgradeBy(-1);
            if (costBefore > 0 && energyCost.GetWithModifiers(CostModifiers.All) == 0 && card.Type != CardType.Power)
                card.AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}