using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Patches;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 战斗内的全局快捷键节点：
/// <list type="bullet">
/// <item>「隐藏 / 显示所有伙伴」（模组设置·键位，默认 C）：所有玩家的伙伴一起隐藏 / 显示；</item>
/// <item>「休眠按钮快捷键」（默认不绑定，避免误触）：等同点击休眠按钮。</item>
/// </list>
/// 绑定字符串来自模组设置，是设置界面规范化后的按键标记（例如 <c>C</c>、<c>Ctrl+C</c>，空串 = 未绑定）。
/// 这里只做按键边沿检测，不消费输入，不影响其它按键。
/// </summary>
public partial class FriendHotkeyWatcher : Node
{
    private bool _toggleWasPressed;
    private bool _sleepWasPressed;

    /// <summary>上一帧「点击队友隐藏」开关的值，用来在设置中途关闭时清理队友的隐藏状态。</summary>
    private bool _teammateClickHideWasEnabled = RainworldModConfig.EnableTeammateClickHide;

    public override void _Process(double delta)
    {
        try
        {
            // 只在战斗场景里处理；离开战斗时本节点会随战斗 UI 一起释放
            if (NCombatRoom.Instance == null)
            {
                _toggleWasPressed = false;
                _sleepWasPressed = false;
                return;
            }

            // 输入框 / 设置界面正在接收键盘输入时不响应，避免打字误触
            if (GetViewport()?.GuiGetFocusOwner() is LineEdit or TextEdit)
            {
                _toggleWasPressed = false;
                _sleepWasPressed = false;
                return;
            }

            // 设置里中途关掉「点击队友隐藏」：立刻把队友的伙伴恢复显示
            bool clickHideEnabled = RainworldModConfig.EnableTeammateClickHide;
            if (_teammateClickHideWasEnabled && !clickHideEnabled)
                FriendVisibility.ClearTeammateHidden();
            _teammateClickHideWasEnabled = clickHideEnabled;

            _toggleWasPressed = HandleBinding(RainworldModConfig.ToggleAllFriendsKey, _toggleWasPressed,
                FriendVisibility.ToggleAll);

            _sleepWasPressed = HandleBinding(RainworldModConfig.SleepHotkey, _sleepWasPressed, () =>
            {
                if (CombatUiPatch.IsSleepAvailable())
                    CombatUiPatch.callSleep();
            });
        }
        catch (Exception e)
        {
            // 快捷键只是便利功能，不能因为异常打断战斗循环
        }
    }

    /// <summary>检测一次按键边沿，返回这一帧按键是否仍被按住。</summary>
    private static bool HandleBinding(string binding, bool wasPressed, Action onPressed)
    {
        bool pressed = IsBindingPressed(binding);
        if (pressed && !wasPressed)
            onPressed();

        return pressed;
    }

    /// <summary>
    /// 绑定字符串是否全部按下：
    /// 键盘组合（如 <c>Ctrl+Shift+C</c>）按每个键逐个判断；
    /// 手柄/输入动作由设置界面录成 <c>action:&lt;动作名&gt;</c>，按 Godot 输入动作判断。
    /// </summary>
    private static bool IsBindingPressed(string binding)
    {
        if (string.IsNullOrWhiteSpace(binding))
            return false;

        const string actionPrefix = "action:";
        if (binding.StartsWith(actionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string action = binding[actionPrefix.Length..].Trim();
            return action.Length > 0 && InputMap.HasAction(action) && Input.IsActionPressed(action);
        }

        foreach (string raw in binding.Split('+'))
        {
            string token = raw.Trim();
            if (token.Length == 0)
                return false;

            Key key = ParseKey(token);
            if (key == Key.None)
                return false;

            if (!Input.IsKeyPressed(key) && !Input.IsPhysicalKeyPressed(key))
                return false;
        }

        return true;
    }

    /// <summary>把设置界面记录下来的标记解析成 Godot 的 <see cref="Key" />。</summary>
    private static Key ParseKey(string token)
    {
        switch (token.ToLowerInvariant())
        {
            case "ctrl":
            case "control":
                return Key.Ctrl;
            case "shift":
                return Key.Shift;
            case "alt":
                return Key.Alt;
            case "meta":
            case "cmd":
            case "command":
                return Key.Meta;
        }

        Key key = OS.FindKeycodeFromString(token);
        return key == Key.None ? OS.FindKeycodeFromString(token.ToUpperInvariant()) : key;
    }
}
