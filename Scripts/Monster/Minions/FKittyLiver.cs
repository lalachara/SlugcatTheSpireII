using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：白猫崽（对应塔1 Kitty_Liver）：攻击 7 并附加创伤，防御给玩家格挡 5，增益给玩家灵巧 2。
/// </summary>
[RegisterMonster]
public sealed class FKittyLiver : FKittyBase
{
    /// <summary>猫崽专用骨架，白猫用 white 皮肤（塔1 setkittyskin("white")）。</summary>
    protected override string VisualsScenePath => "res://Godot/Monster/kittys.tscn";

    protected override string KittySkin => "white";

    protected override int AttackDamage => 7;
    protected override int DefendAmount => 5;
    protected override int BuffAmount => 2;

    /// <summary>攻击附带创伤（攻击回合显示减益意图）。</summary>
    protected override bool AttackHasDebuff => true;

    /// <summary>命中后给目标创伤（1 + 协调层数）。</summary>
    protected override async Task AfterKittyAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        await PowerCmd.Apply<ChuangPower>(choiceContext, target, 1m + TuneAmount(), Creature, null);
    }

    /// <summary>增益态：给玩家灵巧（2 + 协调层数）。</summary>
    protected override async Task PerformKittyBuff(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        await PowerCmd.Apply<NimblePower>(choiceContext, playerCreature, BuffAmount + TuneAmount(), playerCreature, null);
    }
}
