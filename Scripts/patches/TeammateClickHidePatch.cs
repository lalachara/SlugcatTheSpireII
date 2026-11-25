using System;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 点击<b>队友角色</b>隐藏 / 恢复该队友的伙伴（点一次隐藏，再点一次恢复）。
/// <para>实现方式是给非本地玩家的 <see cref="NCreature.Hitbox" /> 连 <c>GuiInput</c> 信号：
/// 只观察左键按下，不消费事件，所以悬停提示、其它输入都不受影响。</para>
/// <para>本地玩家自己的伙伴仍由伙伴护甲面板（<c>CabBlockIndicator</c>）切换；
/// 本地角色不挂监听，避免和「以自己为目标的卡牌」点击冲突。</para>
/// </summary>
public sealed class TeammateClickHidePatch : IPatchMethod
{
    public static string PatchId => "rainworld_teammate_click_hide";

    public static string Description => "点击队友角色隐藏 / 恢复该队友的伙伴";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature), new[] { typeof(Creature) })
    ];

    public static void Postfix(Creature creature)
    {
        try
        {
            if (!creature.IsPlayer || creature.Player is not { } player)
                return;

            // 设置里关掉「点击队友隐藏」后，点击队友不再生效（队友伙伴默认显示）
            if (!RainworldModConfig.EnableTeammateClickHide)
                return;

            // 本地角色不处理：本地玩家的伙伴用护甲面板切换，且避免干扰卡牌指目标
            if (LocalContext.IsMe(creature))
                return;

            NCreature? node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node == null || !GodotObject.IsInstanceValid(node))
                return;

            Control hitbox = node.Hitbox;
            if (hitbox.HasMeta("rainworld_teammate_click_hide"))
                return;

            hitbox.SetMeta("rainworld_teammate_click_hide", true);

            hitbox.GuiInput += @event =>
            {
                if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
                    return;

                if (!GodotObject.IsInstanceValid(hitbox) || !GodotObject.IsInstanceValid(node))
                    return;

                FriendVisibility.Toggle(player);
            };
        }
        catch (Exception e)
        {
            Entry.Logger.Info($"[伙伴显示] 挂载队友点击监听失败：{e.Message}");
        }
    }
}
