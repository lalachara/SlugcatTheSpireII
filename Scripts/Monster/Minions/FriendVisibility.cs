using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Patches;
using Rainworld.Scripts.patches;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴显示开关（按玩家分别记录）：点一下自己的伙伴护甲面板（<c>CabBlockIndicator</c>）
/// 或<b>队友的角色</b>，就把该玩家场上的所有伙伴藏起来，再点一下恢复显示。
/// <para>**只影响显示**：只改 <see cref="CanvasItem.Visible" />，生物本体、血量、意图、
/// 守护承伤（<c>FriendGuardPower</c>）、自动行动（<c>FriendCabAttackPower</c>）全都照常工作。</para>
/// <para>按玩家分开记录（键为 <see cref="Player.NetId" />），多人里隐藏队友的伙伴不会连带
/// 影响自己的伙伴；点击队友角色的监听在 <see cref="patches.TeammateClickHidePatch" />。</para>
/// </summary>
public static class FriendVisibility
{
    /// <summary>被隐藏伙伴的玩家（按 NetId，客户端本地状态，不跨端同步）。</summary>
    private static readonly HashSet<ulong> HiddenPlayerIds = [];

    /// <summary>全局隐藏开关（快捷键，默认 C）：打开后场上所有玩家的伙伴都不显示。</summary>
    private static bool _allHidden;

    /// <summary>开关状态变化时触发（护甲面板据此刷新图标亮度）。</summary>
    public static event Action? Changed;

    /// <summary>全局隐藏开关当前是否打开。</summary>
    public static bool AllHidden => _allHidden;

    /// <summary>本地玩家的伙伴当前是否被隐藏（护甲面板的图标亮度用）。</summary>
    public static bool Hidden =>
        _allHidden || (CombatUiPatch.creature?.Player is { } local && IsHidden(local));

    /// <summary>
    /// 该玩家的伙伴是否被隐藏。
    /// 关闭「点击队友隐藏」后，队友的伙伴默认始终显示（本地玩家自己的单独隐藏仍有效）。
    /// </summary>
    public static bool IsHidden(Player player)
    {
        if (!RainworldModConfig.EnableTeammateClickHide && !LocalContext.IsMe(player))
            return false;

        return HiddenPlayerIds.Contains(player.NetId);
    }

    /// <summary>
    /// 关闭「点击队友隐藏」时清掉所有队友的单独隐藏状态（只保留本地玩家自己那份），
    /// 让队友的伙伴立刻恢复默认显示。
    /// </summary>
    public static void ClearTeammateHidden()
    {
        ulong? localId = LocalContext.NetId;
        int removed = localId.HasValue
            ? HiddenPlayerIds.RemoveWhere(id => id != localId.Value)
            : HiddenPlayerIds.Count;

        if (!localId.HasValue)
            HiddenPlayerIds.Clear();

        if (removed > 0)
        {
            FriendLayout.Apply();
            Changed?.Invoke();
        }
    }

    /// <summary>切换该玩家的伙伴显示 / 隐藏。</summary>
    public static void Toggle(Player player) => Set(player, !IsHidden(player));

    /// <summary>设置该玩家的伙伴显示 / 隐藏，并立刻刷新场上伙伴。</summary>
    public static void Set(Player player, bool hidden)
    {
        // 开关关闭时不允许通过点击队友（或其它途径）单独隐藏队友的伙伴
        if (hidden && !RainworldModConfig.EnableTeammateClickHide && !LocalContext.IsMe(player))
            return;

        bool changed = hidden ? HiddenPlayerIds.Add(player.NetId) : HiddenPlayerIds.Remove(player.NetId);
        if (!changed)
            return;

        FriendLayout.Apply();
        Changed?.Invoke();
    }

    /// <summary>切换全局隐藏开关：所有玩家的伙伴一起隐藏 / 显示（快捷键用）。</summary>
    public static void ToggleAll() => SetAllHidden(!_allHidden);

    /// <summary>设置全局隐藏开关，并立刻刷新场上伙伴。</summary>
    public static void SetAllHidden(bool hidden)
    {
        if (_allHidden == hidden)
            return;

        _allHidden = hidden;
        FriendLayout.Apply();
        Changed?.Invoke();
    }

    /// <summary>切换本地玩家的伙伴显示（护甲面板点击用）。</summary>
    public static void Toggle()
    {
        if (CombatUiPatch.creature?.Player is { } local)
            Toggle(local);
    }

    /// <summary>战斗开始 / 结束或切场景时恢复所有玩家的伙伴显示。</summary>
    public static void Reset()
    {
        if (HiddenPlayerIds.Count == 0 && !_allHidden)
            return;

        HiddenPlayerIds.Clear();
        _allHidden = false;
        FriendLayout.Apply();
        Changed?.Invoke();
    }

    /// <summary>把开关状态套用到单个伙伴节点（按它主人的状态显示 / 隐藏）。</summary>
    public static void ApplyTo(NCreature? node)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        // 两处都未隐藏时才显示：全局快捷键没隐藏，且该玩家自己没被单独隐藏
        if (node.Entity?.PetOwner is { } owner)
            node.Visible = !_allHidden && !IsHidden(owner);
    }

    /// <summary>把开关状态套用到战斗场景里所有本 mod 的伙伴。</summary>
    public static void ApplyToAll()
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
            return;

        foreach (NCreature node in room.CreatureNodes)
        {
            Creature? entity = node.Entity;
            if (entity?.Monster is not FriendPetBase)
                continue;

            // 软退场（RetireFriend）的伙伴已不在宠物列表里，但场景节点故意保留给原版演出用，
            // 这里不能再把它显示回来
            if (entity.PetOwner is not { } owner ||
                FriendPositions.IndexOf(FriendPositions.All(owner), entity) < 0)
                continue;

            ApplyTo(node);
        }
    }
}
