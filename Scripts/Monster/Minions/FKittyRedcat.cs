using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：红猫崽（对应塔1 Kitty_Redcat）：攻击 9，防御给玩家临时生命 4，增益给玩家和自身活力 4。
/// </summary>
[RegisterMonster]
public sealed class FKittyRedcat : FKittyBase
{
    /// <summary>猫崽专用骨架，红猫用 red 皮肤（塔1 setkittyskin("red")）。</summary>
    protected override string VisualsScenePath => "res://Godot/Monster/kittys.tscn";

    protected override string KittySkin => "red";

    protected override int AttackDamage => 9;
    protected override int DefendAmount => 4;
    protected override int BuffAmount => 4;

    /// <summary>防御态：给玩家临时生命（菌壁，4 + 协调层数）。</summary>
    protected override async Task PerformKittyDefend(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        await PowerCmd.Apply<MushblockPower>(choiceContext, playerCreature, DefendAmount + TuneAmount(),
            playerCreature, null);
    }

    /// <summary>增益态：给玩家和自身各一份活力（4 + 协调层数，两边独立消耗）。</summary>
    protected override async Task PerformKittyBuff(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        decimal vigor = BuffAmount + TuneAmount();
        await PowerCmd.Apply<VigorPower>(choiceContext, playerCreature, vigor, playerCreature, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Creature, vigor, Creature, null);
    }

    /// <summary>
    /// 攻击后消耗自身的活力：伙伴攻击不走 AttackCommand 管线，原版的「攻击后消耗」不会触发，
    /// 必须手动移除，否则活力永久累积、猫崽伤害越滚越高（同塔1 Kitty_Redcat 的 RemoveSpecificPowerAction）。
    /// </summary>
    protected override async Task AfterKittyAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        if (Creature.HasPower<VigorPower>())
            await PowerCmd.Remove<VigorPower>(Creature);
    }
}
