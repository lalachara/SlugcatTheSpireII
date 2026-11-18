#if STS2_0_113_0
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Dread;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Monster;

/// <summary>
/// 变异长腿菌的「统御」（恐惧模式，0.113 及以后才有）：关底 boss 影响道中敌人的 buff。
/// 道中的<b>普通 / 精英</b>敌人进入战斗时，最大生命值 +20%（<see cref="MushroomDominionPower" />）。
/// 恐惧模式会在进入关卡时读取当前幕 boss 的 <c>GetDominion()</c>，boss 战本身会被清空统御。
/// </summary>
public sealed class MushroomDominion : DominionModel
{
    /// <summary>最近一次进入的房间类型（进入战斗时基类只把敌人传下来，房间类型得自己在 AfterRoomEntered 里记）。</summary>
    private RoomType? _roomType;

    /// <summary>统御效果直接挂到敌人身上显示：长腿菌「自我增殖」图标 + 说明。</summary>
    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<MushroomDominionPower>()];

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom combatRoom)
            _roomType = combatRoom.Encounter?.RoomType ?? combatRoom.RoomType;

        return base.AfterRoomEntered(room);
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

        await PowerCmd.Apply<MushroomDominionPower>(
            new ThrowingPlayerChoiceContext(), enemies, 1m, null, null);
    }
}
#endif
