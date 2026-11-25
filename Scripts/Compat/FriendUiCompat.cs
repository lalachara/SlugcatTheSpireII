#if !STS2_0_113_0 && !STS2_0_114_0
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Rainworld.Scripts.Compat;

/// <summary>
///     0.111 / 0.111.1 专用：强制把伙伴的「状态显示」（血条 + 能力图标容器，节点名 %HealthBar）显示出来。
///     <para>0.111 的 <see cref="NCreature" /> 在初始化时会按 <c>_isRemotePlayerOrPet</c> 调
///     <c>_stateDisplay.HideImmediately()</c>；对自定义伙伴改完 <c>IsHealthBarVisible</c> 后仍需把
///     已被隐藏的状态显示重新显示（AnimEnableUi 只改 modulate，不动 Visible）。</para>
///     <para>0.113 已移除该机制，所以整段只在 111 编译。</para>
/// </summary>
internal static class FriendUiCompat
{
    public static void EnsureUiVisible(NCreature? node)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        // 血条 + 能力图标都在状态显示节点里，直接显示它
        CanvasItem? stateDisplay = node.GetNodeOrNull<CanvasItem>("%HealthBar");
        if (stateDisplay != null && GodotObject.IsInstanceValid(stateDisplay))
        {
            stateDisplay.Visible = true;
            stateDisplay.Modulate = new Color(1f, 1f, 1f, 1f);
        }

        // 补一次淡入（死亡流程会把 modulate:a 拉到 0）
        node.AnimEnableUi();
    }
}
#endif
