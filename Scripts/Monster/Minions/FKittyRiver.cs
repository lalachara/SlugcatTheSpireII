using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：蓝猫崽（对应塔1 Kitty_River）：攻击 7 并附加侵蚀，防御给玩家格挡 7，增益给玩家灵巧 1。
/// </summary>
[RegisterMonster]
public sealed class FKittyRiver : FKittyBase
{
    /// <summary>猫崽专用骨架，蓝猫用 blue 皮肤（塔1 setkittyskin("blue")）。</summary>
    protected override string VisualsScenePath => "res://Godot/Monster/kittys.tscn";

    protected override string KittySkin => "blue";

    protected override int AttackDamage => 7;
    protected override int DefendAmount => 7;
    protected override int BuffAmount => 1;

    /// <summary>攻击附带侵蚀（攻击回合显示减益意图）。</summary>
    protected override bool AttackHasDebuff => true;

    /// <summary>命中后给目标侵蚀（3 + 协调层数）。</summary>
    protected override async Task AfterKittyAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        await PowerCmd.Apply<CorrodePower>(choiceContext, target, 3m + TuneAmount(), Creature, null);
    }

    /// <summary>增益态：给玩家灵巧（1 + 协调层数）。</summary>
    protected override async Task PerformKittyBuff(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        await PowerCmd.Apply<AdroitPower>(choiceContext, playerCreature, BuffAmount + TuneAmount(), playerCreature, null);
    }
}
