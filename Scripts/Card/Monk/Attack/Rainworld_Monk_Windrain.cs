using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Windrain : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Monk_Strike>() };
    public Rainworld_Monk_Windrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 手牌 + 抽牌堆里的猫猫拳都要打出去（之前只看了抽牌堆，手牌里的完全没动）。
        // 先打手牌：AutoPlay 会把牌从手牌里移走，所以先 ToList 快照再逐张打。
        List<CardModel> strikes = PileType.Hand.GetPile(Owner).Cards
            .Where(IsCatStrike)
            .ToList();
        strikes.AddRange(PileType.Draw.GetPile(Owner).Cards.Where(IsCatStrike));

        foreach (CardModel strike in strikes)
        {
            if (strike.Pile == null)
                continue;

            await CardCmd.AutoPlay(choiceContext, strike, cardPlay.Target);
        }
    }

    /// <summary>猫猫拳：带 Strike 标签、能正常打出，并且不是这张疾风骤雨本身。</summary>
    private static bool IsCatStrike(CardModel card)
        => card.Tags.Contains(CardTag.Strike)
           && card is not Rainworld_Monk_Windrain
           && !card.Keywords.Contains(CardKeyword.Unplayable);

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}