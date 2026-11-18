using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 挂在本地玩家 NCreature 下的观察者：定时按鼠标位置重算「是否有玩家被悬停」。
/// 只用来兜底输入信号 —— 多人里队友角色的 Hitbox 常被意图图标、名字标签等控件压住，
/// MouseEntered/Exited 收不到，光靠 OnFocus/OnUnfocus 会漏掉悬停队友的情况。
/// </summary>
public partial class PetHoverWatcher : Node
{
    /// <summary>轮询间隔（秒）：比淡入淡出时长（0.12s）短，肉眼上仍是即时响应。</summary>
    private const double IntervalSeconds = 0.05;

    private double _elapsed;

    /// <summary>上一次轮询的结果，用来避免反复起补间。</summary>
    private bool _faded;

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed < IntervalSeconds)
            return;
        _elapsed = 0;

        if (GetParent() is not NCreature parent || parent.Entity?.CombatState is not { } combatState)
            return;

        bool hovered = MinionHoverFadePatch.IsAnyPlayerHovered(combatState);
        if (hovered == _faded)
            return;

        _faded = hovered;
        MinionHoverFadePatch.SetPetsFaded(combatState, hovered);
    }
}
