#if STS2_0_113_0 || STS2_0_114_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Dread;
using MegaCrit.Sts2.Core.Rooms;

namespace Rainworld.Scripts.Monster;

/// <summary>
/// 变异长腿菌的「统御」（恐惧模式，0.113 及以后才有）：关底 boss 影响道中敌人的 buff。
/// 道中的<b>普通 / 精英</b>敌人进入战斗时，最大生命值 +20%。
/// <para>直接用 <see cref="CreatureCmd.SetMaxHp" /> / <see cref="CreatureCmd.Heal" /> 改敌人的血条，
/// <b>不给敌人挂任何能力图标</b>。统御的标题 / 说明走 <c>DominionModel.Description</c>
/// 读的 ascension 表（键名 <c>DREAD.MUSHROOM.title / .description</c>）。</para>
/// <para>恐惧模式会在进入关卡时读取当前幕 boss 的 <c>GetDominion()</c>，boss 战本身会被清空统御。</para>
/// </summary>
public sealed class MushroomDominion : DominionModel
{
    /// <summary>最大生命提升比例（20%）。</summary>
    public const decimal MaxHpBonusRatio = 0.25m;

    /// <summary>最近一次进入的房间类型（进入战斗时基类只把敌人传下来，房间类型得自己在 AfterRoomEntered 里记）。</summary>
    private RoomType? _roomType;

    /// <summary>本场战斗里已经加过血的敌人（敌人在 AfterEnemiesEnteredCombat 与 AfterCreatureAddedToCombat 两条路径下都可能来一次）。</summary>
    private readonly HashSet<Creature> _buffed = [];

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom combatRoom)
            _roomType = combatRoom.Encounter?.RoomType ?? combatRoom.RoomType;

        return base.AfterRoomEntered(room);
    }

    /// <summary>新一场战斗开始：清掉上一场的加血记录。</summary>
    public override async Task BeforeCombatStart()
    {
        _buffed.Clear();
        await base.BeforeCombatStart();
    }

    /// <summary>只对普通怪物 / 精英的遭遇生效，其它（事件、休息点、boss 战）都不加。</summary>
    protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
    {
        if (_roomType is not (RoomType.Monster or RoomType.Elite))
            return;

        var enemies = creatures
            .Where(c => c != null && c.IsAlive && c.Side == CombatSide.Enemy)
            .ToList();

        if (enemies.Count == 0)
            return;

        foreach (Creature enemy in enemies)
        {
            // 同一场战斗里只加一次（房间进入 + 生物加入战斗两条路径可能都触发）
            if (!_buffed.Add(enemy))
                continue;

            // 提升量 = ceil(最大生命 × 20%)，并把提升的这部分补满（和原来的能力行为一致）
            decimal bonus = Math.Ceiling(enemy.MaxHp * MaxHpBonusRatio);
            if (bonus <= 0m)
                continue;

            await CreatureCmd.SetMaxHp(enemy, enemy.MaxHp + bonus);
            await CreatureCmd.Heal(enemy, bonus);
        }
    }
}
#endif
