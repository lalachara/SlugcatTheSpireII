using System;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Rainworld.Scripts.Compat;

/// <summary>
///     0.113 兼容层：<see cref="NCreature" /> 移除了 ToggleIsInteractable(bool) 与 AnimEnableUi()，
///     改为 <c>SetIsInteractable(bool on, bool changeVisibilityImmediately)</c>。
///
///     注意：死亡流程会先把私有的 <c>_interactionAllowedBecauseAlive</c> 置为 false，
///     此时直接调 <c>SetIsInteractable(true, true)</c> 依然会被判成不可交互，
///     所以这里先把这个开关翻回来，再恢复交互（等价旧版 ToggleIsInteractable(true) + AnimEnableUi()）。
/// </summary>
internal static class CreatureNodeCompat
{
    private static readonly FieldInfo? InteractionAllowedBecauseAliveField =
        typeof(NCreature).GetField("_interactionAllowedBecauseAlive", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>开启/关闭悬停交互，并同步血条/状态条显示。</summary>
    public static void SetInteractable(NCreature? node, bool on)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        try
        {
            InteractionAllowedBecauseAliveField?.SetValue(node, on);
            node.SetIsInteractable(on, true);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[0.113 兼容] 设置互动状态失败: {ex.Message}");
        }
    }
}
