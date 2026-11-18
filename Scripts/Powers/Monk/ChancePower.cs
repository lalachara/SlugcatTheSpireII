using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 把握时机：你使用的下 Amount 张带有保留的牌不耗能（每打出一张带保留的牌就消耗 1 层，0 费牌同样消耗）。
/// 未用完的层数会保留到之后的回合，直到打出足够数量的保留牌。
/// </summary>
[RegisterPower]
public sealed class ChancePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/timing.png",
        BigIconPath: "res://Resource/Powers/Monk/timing.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner)
            return false;
        if (!card.ShouldRetainThisTurn)
            return false;

        switch (card.Pile?.Type)
        {
            case PileType.Hand:
            case PileType.Play:
                modifiedCost = 0m;
                return true;
            default:
                return false;
        }
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
            return;
        if (!cardPlay.Card.ShouldRetainThisTurn)
            return;
        // 只要打出的是「带保留的牌」就消耗 1 层，不管它本身是不是 0 费。
        // 之前放行「本来就是 0 费」的牌，会和各种改费来源（投石/其他 0 费效果/单卡降费）打架，
        // 出现「层数永远消耗不掉」的问题；统一一律消耗，行为最可预期。
        await PowerCmd.Decrement(this);
    }
    
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)	{
        if (side == Owner.Side)
            await PowerCmd.Remove(this);
    }
}
