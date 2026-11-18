using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 失去协调（对应塔1 LoseTunePower）：下个己方回合开始时，失去与层数等量的「协调」。
/// 配合「战前动员」等卡实现「本回合临时获得协调、下回合归还」的效果。
/// </summary>
[RegisterPower]
public sealed class LoseTunePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Tunelose128.png",
        BigIconPath: "res://Resource/Powers/Monk/Tunelose128.png");

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>下个己方回合开始时结算：先移除自身，再扣除等量「协调」。</summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side || !Owner.IsAlive)
            return;

        decimal loss = Amount;
        await PowerCmd.Remove(this);

        if (Owner.GetPower<TunePower>() is not { } tune)
            return;

        if (tune.Amount - loss <= 0m)
            await PowerCmd.Remove(tune);
        else
            await PowerCmd.ModifyAmount(new BlockingPlayerChoiceContext(), tune, -loss, Owner, null);
    }
}
