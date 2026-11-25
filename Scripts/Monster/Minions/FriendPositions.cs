using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Scripts.Compat;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴站位（模仿塔1 AbstractFriend 的固定站位逻辑）：
/// 前 5 只伙伴按固定顺序依次站到五个站位（右上方 → 左上方 → 正上方 → 右侧 → 左侧），
/// 第 6 只起循环复用，同一站位上的多只由 <see cref="FriendLayout" /> 自动错开。
/// <para>数据来源就是原版的 <c>PlayerCombatState.Pets</c>（官方宠物列表，Byrdpip / Wuppy 用的同一份），
/// 不再依赖 MinionLib 的 <c>PetsOrderAccessor</c>。</para>
/// </summary>
public static class FriendPositions
{
    /// <summary>塔1 FIXED_OFFSETS 的站位顺序：先占高位三角，再补前后排。</summary>
    private static readonly FriendPosition[] FixedOrder =
    [
        FriendPosition.FrontUpper,
        FriendPosition.BackUpper,
        FriendPosition.Upper,
        FriendPosition.Front,
        FriendPosition.Back
    ];

    /// <summary>玩家当前的伙伴列表（无伙伴时为空白列表）。</summary>
    public static IReadOnlyList<Creature> All(Player player) =>
        player.PlayerCombatState?.Pets ?? [];

    /// <summary>
    /// 宠物列表的可写引用：原版 <c>PlayerCombatState.Pets</c> 返回值底层就是 <c>List&lt;Creature&gt;</c>，
    /// 「让伙伴直接退场」必须就地移除，否则它仍会被当成宠物继续参与遍历。
    /// </summary>
    public static List<Creature>? RawList(Player player) =>
        player.PlayerCombatState?.Pets as List<Creature>;

    /// <summary>伙伴在玩家宠物列表中的序号（找不到返回 -1）。</summary>
    public static int IndexOf(IReadOnlyList<Creature> pets, Creature creature)
    {
        for (int i = 0; i < pets.Count; i++)
        {
            if (ReferenceEquals(pets[i], creature))
                return i;
        }

        return -1;
    }

    /// <summary>玩家当前的伙伴数量。</summary>
    public static int Count(Player player) => All(player).Count;

    /// <summary>
    /// 玩家当前的本 mod 伙伴（拾荒者、蜥蜴、猫崽等）。
    /// 原版宠物（异鸟、瓦皮、Pael's Legion 等）与其它 mod 的随从都不计入，
    /// 避免它们被当成「伙伴」参与治疗、执行意图等结算。
    /// </summary>
    public static List<Creature> ModFriends(Player player) =>
        All(player).Where(pet => pet.Monster is FriendPetBase).ToList();

    /// <summary>
    /// 该玩家是否有「未处于眩晕」的伙伴：存活且未休眠（倒地等待复活的不算）。
    /// 只统计本 mod 的伙伴，别家随从不计入。
    /// </summary>
    public static bool HasAwakeFriend(Player player)
    {
        foreach (Creature pet in All(player))
        {
            if (pet.Monster is not FriendPetBase)
                continue;
            if (pet.IsAlive && pet.GetPower<FriendDormantPower>() is not { IsDormant: true })
                return true;
        }

        return false;
    }

    /// <summary>第 index 只伙伴的站位：按固定顺序取，第 6 只起循环。</summary>
    public static FriendPosition At(int index) => FixedOrder[index % FixedOrder.Length];

    /// <summary>下一只伙伴的站位：按当前数量取固定顺序中的下一个（第 6 只起循环）。</summary>
    public static FriendPosition Next(Player player) =>
        At(Count(player));

    /// <summary>
    /// 让一只伙伴直接退场：不触发死亡，也不进入休眠复活。
    /// 手动镜像官方 <c>CreatureCmd.Escape</c> 的流程，但跳过其「逃离者」登记——
    /// 随从若被记入 EscapedCreatures，战斗金币奖励会按逃离比例扣减。
    /// </summary>
    public static void LoseFriend(Player player, Creature friend)
    {
        RawList(player)?.Remove(friend);

        // 移除全部能力（守护、休眠倒计时等随退场一并失效，避免留在场上继续复活）
        friend.RemoveAllPowersInternalExcept(null);

        // 从场景中移除节点
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? node = room?.GetCreatureNode(friend);
        if (room != null && node != null)
        {
            room.RemoveCreatureNode(node);
            SetNodeInteractable(node, false);
            node.Visible = false;
        }

        // 从战斗状态中移除
        CombatManager.Instance?.RemoveCreature(friend);
        ICombatState? combatState = friend.CombatState;
        if (combatState != null && combatState.ContainsCreature(friend))
            combatState.RemoveCreature(friend, true);
    }

    /// <summary>
    /// 软退场：主人战死后把伙伴撤下，但<b>保留场景节点</b>（只隐藏），不调用
    /// <c>NCombatRoom.RemoveCreatureNode</c>。
    /// <para>原因：原版无厌沙虫的 <c>SandpitPower.AfterRemoved</c> 会在一个 foreach 里先
    /// <c>Kill</c> 被吞玩家、再遍历当时快照下来的宠物数组；杀玩家会同步触发
    /// <c>HandlePlayerDeath</c>。如果这时我们立刻销毁宠物节点，沙虫那边仍拿着旧快照，
    /// <c>NCombatRoom.GetCreatureNode</c> 返回 null 之后马上访问 <c>.Visuals</c> → 空引用，
    /// 表现为「队友被沙虫吞掉后整局卡死」。保留节点就不会走到 null。</para>
    /// <para>节点会在战斗结束、场景清理时统一销毁，不需要额外收尾。</para>
    /// </summary>
    public static void RetireFriend(Player player, Creature friend)
    {
        RawList(player)?.Remove(friend);

        friend.RemoveAllPowersInternalExcept(null);

        // 只隐藏，不摘节点
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? node = room?.GetCreatureNode(friend);
        if (node != null)
        {
            node.Visible = false;
            SetNodeInteractable(node, false);
        }

        CombatManager.Instance?.RemoveCreature(friend);
        ICombatState? combatState = friend.CombatState;
        if (combatState != null && combatState.ContainsCreature(friend))
            combatState.RemoveCreature(friend, true);
    }
    /// <summary>关闭节点上的悬停交互（0.113 用 SetIsInteractable，0.111 用 ToggleIsInteractable + AnimDisableUi）。</summary>
    private static void SetNodeInteractable(NCreature node, bool on)
    {
#if STS2_0_113_0 || STS2_0_114_0
        CreatureNodeCompat.SetInteractable(node, on);
#else
        node.ToggleIsInteractable(on);
        if (!on)
            node.AnimDisableUi();
#endif
    }

}
