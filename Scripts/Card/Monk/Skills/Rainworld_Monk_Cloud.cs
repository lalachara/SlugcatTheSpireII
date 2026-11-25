using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Cloud : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private static readonly PileType[] countPiles = { PileType.Draw, PileType.Discard, PileType.Hand, PileType.Play, PileType.Exhaust };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Colorful };

    public override bool GainsBlock => true;

    /// <summary>
    /// 卡组里实际会算进格挡的颜色数：抽牌堆 + 弃牌堆 + 手牌 + 出牌堆 + 消耗堆，
    /// 也就是「这一场战斗里的全部卡牌」——卡面写的是「卡组中每有 1 种颜色」，
    /// 只看抽/弃/手的话，进阶之灾这类 Ethereal 诅咒在自己消耗掉之后就不见了，
    /// 一次战斗里颜色数会从 2 掉回 1。
    /// 普通牌按卡池去重，彩虹牌每张额外 +1 种，自身不计入。
    /// 卡面（当前颜色数）与实际结算共用这一份口径。
    /// </summary>
    private static decimal CountColors(CardModel card, Creature? creature)
    {
        if (card is not Rainworld_Monk_Cloud || card.Owner == null)
            return 0m;

        var colors = new HashSet<CardPoolModel>();
        var colorfulCount = 0;

        foreach (var pileType in countPiles)
        {
            foreach (var stacked in pileType.GetPile(card.Owner).Cards)
            {
                if (stacked == card)
                    continue;
                if (stacked.Keywords.Contains(RainworldKeywords.Colorful))
                {
                    colorfulCount++;
                    continue;
                }

                var pool = stacked.VisualCardPool;
                if (pool != null)
                    colors.Add(pool);
            }
        }

        return colors.Count + colorfulCount;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedColors").WithMultiplier(CountColors)
    ];

    public Rainworld_Monk_Cloud() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var total = (int)((CalculatedVar)DynamicVars["CalculatedColors"]).Calculate(null);
        if (total > 0)
            for (var i = 0; i < total; i++)
            {
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
            }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1m);
    }
}
