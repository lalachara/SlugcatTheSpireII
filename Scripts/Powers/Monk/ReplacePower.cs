using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 置换反应留在目标身上的标记：玩家回合结束时，目标把手里剩下的格挡全部换成等量生命。
/// 这跟格挡是怎么来的无关（置换反应发的、目标自己叠的、别人给的，一律一起换）。
/// 标记本身没有层数：无论先后打出几张置换反应，回合结束时都只结算同一次转换。
/// </summary>
[RegisterPower]
public sealed class ReplacePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/replace128.png",
        BigIconPath: "res://Resource/Powers/Monk/replace128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 结算点必须是玩家回合结束时（怪物行动之前）：怪物的格挡会在它自己的回合里被清掉，
    /// 等到怪物行动之后再来读格挡就只剩 0，等量生命永远回不出来。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;
        if (CombatManager.Instance.IsOverOrEnding)
            return;
        // 已经没血的目标没什么好回的，只把标记清掉
        if (!Owner.IsDead && Owner.CurrentHp > 0)
        {
            var block = Owner.Block; // 全部剩余格挡，不看来源
            if (block > 0)
            {
                await CreatureCmd.LoseBlock(choiceContext, Owner, block, null);
                await CreatureCmd.Heal(Owner, block);
            }
        }
        await PowerCmd.Remove(this);
    }
}
