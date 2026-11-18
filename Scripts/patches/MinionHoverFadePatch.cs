using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 鼠标悬停在任意玩家身上时，把场上的伙伴（随从）整体淡出，并隐藏伙伴的血条、能力图标等 UI；移开后恢复。
/// 移植塔1 AbstractFriend 的悬停淡出（PLAYER_HOVER_ALPHA = 0.35）：伙伴都挤在角色周围，
/// 查看角色信息时不能挡着。
/// 多人：悬停自己或队友的节点都生效，且淡出的是场上全部伙伴（含队友的）——
/// 只看自己的会让队友的伙伴继续挡在画面上。
/// </summary>
/// <remarks>
/// 触发有两条路径：NCreature.OnFocus/OnUnfocus（Hitbox 的 MouseEntered/Exited 与键盘焦点），
/// 以及挂在本地玩家节点上的 <see cref="PetHoverWatcher" /> 按鼠标位置轮询。
/// 后者是必需的：多人里队友角色的 Hitbox 常被意图图标、名字标签等控件压住，
/// MouseEntered 收不到，只靠信号会漏掉悬停队友的情况。
/// </remarks>
public sealed class MinionHoverFadePatch : IPatchMethod
{
    public static string PatchId => "rainworld_minion_hover_fade";
    public static string Description => "悬停自己或队友时伙伴整体淡出";
    public static bool IsCritical => false;

    /// <summary>淡出时的目标透明度（与塔1一致）</summary>
    private const float HoverAlpha = 0.35f;

    /// <summary>渐隐渐现时长（秒）</summary>
    private const float FadeSeconds = 0.12f;

    /// <summary>记录节点上一次的淡入淡出补间，快速进出悬停时把旧的顶掉</summary>
    private const string TweenMeta = "rainworld_minion_fade_tween";

    /// <summary>记录隐藏 UI 前的可见性，移开悬停时据此恢复（悬停期间倒下的伙伴保持隐藏）</summary>
    private const string UiVisibleMeta = "rainworld_minion_ui_visible";

    /// <summary>轮询观察者的节点名（挂在本地玩家的 NCreature 下）</summary>
    private const string WatcherName = "RainworldPetHoverWatcher";

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCreature), "OnFocus"),
        new(typeof(NCreature), "OnUnfocus"),
        new(typeof(NCreature), "_Ready"),
    ];

    public static void Postfix(NCreature __instance)
    {
        var entity = __instance.Entity;
        // 任意玩家的节点都处理（自己与队友都要让伙伴让开）
        if (entity == null || !entity.IsPlayer)
            return;

        // 本地玩家节点上挂一个轮询观察者，兜底队友节点收不到鼠标事件的情况
        if (LocalContext.IsMe(entity) && !__instance.HasNode(WatcherName))
            __instance.AddChild(new PetHoverWatcher { Name = WatcherName });

        // 鼠标一次只停在一个节点上，但 OnUnfocus 时键盘焦点可能离开而鼠标还停在角色身上
        // （FocusExited 与 MouseExited 各触发一次），所以不看当前事件来自哪个节点，
        // 而是把全体玩家节点重新扫一遍：只要还有任意一个被悬停，就保持淡出。
        UpdateFor(entity.CombatState);
    }

    /// <summary>按当前焦点/鼠标位置重新判定一次并同步伙伴状态（信号与轮询共用）。</summary>
    private static void UpdateFor(ICombatState? combatState)
    {
        if (combatState == null)
            return;
        SetPetsFaded(combatState, IsAnyPlayerHovered(combatState));
    }

    /// <summary>是否有任意玩家节点正被悬停：键盘焦点，或鼠标落在它的 Hitbox 里。</summary>
    internal static bool IsAnyPlayerHovered(ICombatState combatState)
    {
        foreach (Creature playerCreature in combatState.PlayerCreatures)
        {
            if (playerCreature.GetCreatureNode() is { } node && IsHovered(node))
                return true;
        }
        return false;
    }

    /// <summary>节点是否正被悬停：键盘焦点，或鼠标落在它的 Hitbox 里。</summary>
    private static bool IsHovered(NCreature node) =>
        node.IsFocused
        || (node.Hitbox != null && node.Hitbox.GetGlobalRect()
            .HasPoint(node.GetGlobalMousePosition()));

    /// <summary>
    /// 把场上所有伙伴节点整体淡出/淡回，并隐藏/恢复它们的 UI。
    /// 只处理本 mod 的伙伴（<see cref="FriendPetBase" />）：别的随从（如原版 Osty）不归这里管。
    /// 从全场 Allies 里筛，而不是遍历各玩家的 Pets —— 队友的 PlayerCombatState 在本地未必完整。
    /// </summary>
    internal static void SetPetsFaded(ICombatState combatState, bool faded)
    {
        foreach (Creature creature in combatState.Allies)
        {
            if (creature.Monster is not FriendPetBase)
                continue;

            // Creature.GetCreatureNode 内部兼顾战斗房间与图鉴场景，拿不到节点（如已阵亡）时返回 null
            if (creature.GetCreatureNode() is not { } petNode)
                continue;

            FadeTo(petNode, faded ? HoverAlpha : 1f);
            SetUiHidden(petNode, faded);
        }
    }

    /// <summary>
    /// 悬停时把伙伴的血条、能力图标等 UI 隐藏：它们都在 NCreature 的 %HealthBar（NCreatureStateDisplay）节点下。
    /// 整节点淡到 0.35 时 UI 依旧很显眼，所以这里直接关掉可见性；移开时按悬停前的可见性恢复。
    /// </summary>
    private static void SetUiHidden(NCreature node, bool hidden)
    {
        if (node.GetNodeOrNull<NCreatureStateDisplay>("%HealthBar") is not { } display)
            return;

        if (hidden)
        {
            // 只在第一次隐藏时记录原状态，避免连续 hover 事件覆盖成 false
            if (!node.HasMeta(UiVisibleMeta))
                node.SetMeta(UiVisibleMeta, display.Visible);
            display.Visible = false;
            return;
        }

        bool wasVisible = node.HasMeta(UiVisibleMeta) && node.GetMeta(UiVisibleMeta).AsBool();
        node.RemoveMeta(UiVisibleMeta);
        // 悬停期间伙伴倒下时死亡动画会接管 UI 的显隐，此时不要强行打开
        if (wasVisible && node.Entity is { IsAlive: true })
            display.Visible = true;
    }

    private static void FadeTo(NCreature node, float alpha)
    {
        // 替换上一次还没播完的补间，避免快速进出悬停时两段补间同时改 modulate 互相打架
        if (node.HasMeta(TweenMeta) && node.GetMeta(TweenMeta).As<Tween>() is { } oldTween
            && GodotObject.IsInstanceValid(oldTween))
        {
            oldTween.Kill();
        }

        Tween tween = node.CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        // 只动 modulate 的 alpha 通道，保留 rgb（不覆盖其它地方对配色的调整）
        tween.TweenProperty(node, "modulate:a", alpha, FadeSeconds);
        node.SetMeta(TweenMeta, tween);
    }
}
