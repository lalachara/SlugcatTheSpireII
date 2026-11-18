using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Rainworld.Scripts.Vfx;

/// <summary>
/// 原版「爆炸」能力（ExplosivePower）引爆时的火焰烟雾特效（NFireSmokePuffVfx）。
/// 爆炸矛第二段、手雷的爆炸都复用它：把特效挂在被炸到的敌人身上。
/// </summary>
public static class BombBurstVfx
{
    /// <summary>在给定目标身上逐个播放原版爆炸火焰烟雾特效。</summary>
    public static void Play(IEnumerable<Creature>? targets)
    {
        if (targets == null)
            return;

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
            return;

        Node container = room.CombatVfxContainer;
        if (container == null || !GodotObject.IsInstanceValid(container))
            return;

        foreach (Creature target in targets)
        {
            if (target == null || !target.IsAlive || target.GetCreatureNode() == null)
                continue;

            container.AddChildSafely(NFireSmokePuffVfx.Create(target));
        }
    }
}
