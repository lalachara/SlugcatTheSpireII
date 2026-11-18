using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;
// 0.113 游戏本体新增了同名 TunePower，这里显式指定本 mod 的协调能力
using TunePower = Rainworld.Scripts.Powers.TunePower;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：黄猫崽（对应塔1 Kitty_Monk）：攻击 6 并让目标失去 1 点力量（塔1 的「创伤」），
/// 防御给玩家格挡 6，增益给玩家协调 1（塔1 这个增益不吃协调加成）。
/// </summary>
[RegisterMonster]
public sealed class FKittyMonk : FKittyBase
{
    /// <summary>猫崽专用骨架：slugpup（与塔1 setkittyskin 同一套），动作只有 idle_loop / attack。</summary>
    protected override string VisualsScenePath => "res://Godot/Monster/kittys.tscn";

    protected override string KittySkin => "yellow";

    protected override int AttackDamage => 6;
    protected override int DefendAmount => 6;
    protected override int BuffAmount => 1;

    /// <summary>攻击附带创伤（攻击回合显示减益意图）。</summary>
    protected override bool AttackHasDebuff => true;

    /// <summary>命中后让目标失去 1 点力量（塔1：new StrengthPower(target, -woundBase)，不随协调成长）。</summary>
    protected override async Task AfterKittyAttack(PlayerChoiceContext choiceContext, Creature target)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, target, -1m, Creature, null);
    }

    /// <summary>增益态：给玩家协调 1（塔1：new TunePower(player, nimbleBase)，不随协调成长）。</summary>
    protected override async Task PerformKittyBuff(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        await PowerCmd.Apply<TunePower>(choiceContext, playerCreature, BuffAmount, playerCreature, null);
    }
}
