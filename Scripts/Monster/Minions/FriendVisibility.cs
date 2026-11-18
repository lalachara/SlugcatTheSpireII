using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴显示开关：点一下伙伴护甲面板（<c>CabBlockIndicator</c>）就把场上所有伙伴整体藏起来，
/// 再点一下恢复显示。
/// <para>**只影响显示**：只改 <see cref="CanvasItem.Visible" />，生物本体、血量、意图、
/// 守护承伤（<c>FriendGuardPower</c>）、自动行动（<c>FriendCabAttackPower</c>）全都照常工作。
/// 伙伴挤在玩家周围会挡视线，需要看清战场时把她们收起来而已。</para>
/// <para>摆放位置/层级仍由 <see cref="FriendLayout" /> 负责：每次重新摆位都会把可见性
/// 按当前开关状态再刷一遍，所以隐藏期间新召唤的伙伴也同样是隐藏的。</para>
/// </summary>
public static class FriendVisibility
{
    /// <summary>当前是否隐藏全部伙伴。</summary>
    public static bool Hidden { get; private set; }

    /// <summary>开关状态变化时触发（护甲面板据此刷新图标亮度）。</summary>
    public static event Action? Changed;

    /// <summary>切换隐藏 / 显示。</summary>
    public static void Toggle() => Set(!Hidden);

    /// <summary>设置隐藏 / 显示，并立刻刷新场上伙伴。</summary>
    public static void Set(bool hidden)
    {
        if (Hidden == hidden)
            return;

        Hidden = hidden;
        FriendLayout.Apply();
        Changed?.Invoke();
    }

    /// <summary>战斗开始 / 结束或切场景时恢复显示，避免下一场战斗一起手就是空的。</summary>
    public static void Reset() => Set(false);

    /// <summary>把开关状态套用到单个伙伴节点（显示 / 隐藏）。</summary>
    public static void ApplyTo(NCreature? node)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        node.Visible = !Hidden;
    }

    /// <summary>把开关状态套用到战斗场景里所有本 mod 的伙伴。</summary>
    public static void ApplyToAll()
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
            return;

        foreach (NCreature node in room.CreatureNodes)
        {
            if (node.Entity?.Monster is FriendPetBase)
                ApplyTo(node);
        }
    }
}
