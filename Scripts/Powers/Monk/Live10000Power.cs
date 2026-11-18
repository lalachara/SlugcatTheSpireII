using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 万籁共生：每当你增援一只新伙伴时，获得与层数等量的能量。
/// </summary>
[RegisterPower]
public sealed class Live10000Power : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/w128.png",
        BigIconPath: "res://Resource/Powers/Monk/w128.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        // 只响应自己增援的伙伴，不响应敌人召唤或其它玩家的伙伴
        if (Owner.Player == null || creature.PetOwner != Owner.Player)
            return;
        if (creature.Monster is not FriendPetBase)
            return;

        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
    }
}
