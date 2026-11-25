using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Scripts.Compat;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴站位布局：原生实现，替代原先 MinionLib 的 <c>IMinionLayout</c> + <c>MinionLayoutManager</c>。
/// <para>原版对宠物只有一套排版（<c>NCombatRoom.AddCreature</c> / <c>PositionPlayersAndPets</c>：
/// 贴着主人脚印排成一排），而塔1 的伙伴是五个固定站位，所以这里在引擎摆完之后自己再摆一次。</para>
/// <para>坐标与旧版完全一致：伙伴相对主人节点的像素偏移，
/// 右上方 84,-115（第 1 只，已调近并右移） / 左上方 -85,-150 / 正上方 0,-200 / 右侧 140,10 / 左侧 -140,10，
/// 同一站位多出来的伙伴按 55,-30 依次错开。</para>
/// <para>只处理本 mod 的伙伴（<see cref="FriendPetBase" />）；原版 Osty / Wuppy 与其它 mod 的随从
/// 仍走原版排版，互不干扰。</para>
/// </summary>
public static class FriendLayout
{
    /// <summary>
    /// 各站位的基准偏移（相对玩家节点的像素坐标；Godot 坐标 X 向右为正、Y 向下为正）。
    /// 调整伙伴离玩家的距离只改这里：Y 越小（越负）离玩家头顶越远，X 绝对值越大离玩家越远。
    /// </summary>
    private static readonly Dictionary<FriendPosition, Vector2> BaseOffsets = new()
    {
        [FriendPosition.Front] = new Vector2(140f, 10f), // 右侧（第 4 只）
        [FriendPosition.Back] = new Vector2(-140f, 10f), // 左侧（第 5 只）
        [FriendPosition.FrontUpper] = new Vector2(84f, -115f), // 右上方（第 1 只）——离玩家更近、再往右挪小半个身位
        [FriendPosition.BackUpper] = new Vector2(-85f, -150f), // 左上方（第 2 只）
        [FriendPosition.Upper] = new Vector2(0f, -200f) // 正上方（第 3 只）
    };

    /// <summary>同一站位有多只时（第 6 只起循环复用）每多一只额外错开的偏移，避免完全重叠。</summary>
    private static readonly Vector2 StackOffset = new(55f, -30f);

    /// <summary>
    /// 把场上所有本 mod 伙伴按五站位摆好（含休眠/倒地的伙伴）。
    /// 由 <c>FriendPetLayoutPatch</c> 挂在原版三个摆位时机之后调用：
    /// 生物进场（<c>NCombatRoom.AddCreature</c>）、节点移除（<c>NCombatRoom.RemoveCreatureNode</c>）、
    /// 战斗开始整体排版（<c>NCombatRoom.PositionPlayersAndPets</c>）。
    /// </summary>
    public static void Apply()
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
            return;

        // 按主人分组：只挑本 mod 的伙伴，并且必须有主人（原版宠物都满足 PetOwner != null）
        Dictionary<Player, List<NCreature>> byOwner = new();

        foreach (NCreature node in room.CreatureNodes)
        {
            if (!GodotObject.IsInstanceValid(node))
                continue;

            Creature? entity = node.Entity;
            if (entity?.Monster is not FriendPetBase || entity.PetOwner is not { } owner)
                continue;

            if (!byOwner.TryGetValue(owner, out List<NCreature>? nodes))
                byOwner[owner] = nodes = [];

            nodes.Add(node);
        }

        foreach ((Player owner, List<NCreature> nodes) in byOwner)
        {
            NCreature? ownerNode = room.GetCreatureNode(owner.Creature);
            if (ownerNode == null || !GodotObject.IsInstanceValid(ownerNode))
                continue;

            IReadOnlyList<Creature> pets = FriendPositions.All(owner);

            // 同一站位内的序号（第 6 只起循环复用站位，需要错开）
            Dictionary<FriendPosition, int> stackCounts = new();

            foreach (NCreature node in nodes.OrderBy(n => FriendPositions.IndexOf(pets, n.Entity)))
            {
                int index = FriendPositions.IndexOf(pets, node.Entity);
                if (index < 0)
                    continue;

                FriendPosition slot = FriendPositions.At(index);
                int stack = stackCounts.TryGetValue(slot, out int count) ? count : 0;
                stackCounts[slot] = stack + 1;

                Vector2 offset = BaseOffsets[slot] + StackOffset * stack;
                node.Position = ownerNode.Position + offset;

                // 显示开关：玩家点伙伴护甲面板收起伙伴时，隐藏期间新召唤的伙伴也要一起藏起来
                FriendVisibility.ApplyTo(node);

#if !STS2_0_113_0 && !STS2_0_114_0
                // 0.111 专用：把血条/能力图标所在的状态显示强制打开
                FriendUiCompat.EnsureUiVisible(node);
#endif

                // 休眠/倒地的伙伴要保留悬停交互，玩家才能看到复活倒计时
                if (node.Entity?.Monster is FriendPetBase { IsDormant: true })
                    CreatureNodeCompat.SetInteractable(node, true);
            }

            // 图层：整组「主人 + 伙伴」按 Y 升序统一排序（越靠上画得越早 → 图层越靠下）
            ApplyDepth(ownerNode, nodes);
        }
    }

    /// <summary>召唤 / 重排后调用：把伙伴摆回各自的站位（等价于 <see cref="Apply" />，保留异步签名方便链路调用）。</summary>
    public static Task RearrangeAsync()
    {
        Apply();
        return Task.CompletedTask;
    }

    /// <summary>把本 mod 的伙伴（含休眠/倒地的）摆回站位：兼容旧补丁名，内部就是 <see cref="Apply" />。</summary>
    public static void RestorePetPositions() => Apply();

    /// <summary>按「在玩家上方则画在下层、在下方则画在上层」调整兄弟节点顺序。</summary>
    /// <summary>
    /// 图层排序：伙伴按<b>屏幕 Y 升序</b>重排兄弟节点顺序——越靠画面上方（Y 越小）越先绘制，
    /// 也就是<b>图层越靠下</b>（被下方伙伴盖住），不会再出现上方伙伴压住下方伙伴的乱遮挡。
    /// <para>关键：以「伙伴自己的父容器」为准分组排序（玩家节点不一定和伙伴在同一个容器里，
    /// 之前按主人容器过滤会导致一个组都没排上、实际仍是召唤顺序）。如果主人确实同容器，也一起参与排序。</para>
    /// <para>不能改 ZIndex：伙伴所在的 AllyContainer 在 CombatSceneContainer（ZIndex = -10）里，
    /// 设成负值会连玩家一起沉到背景之下，所以统一用兄弟节点顺序控制绘制先后。</para>
    /// </summary>
    private static void ApplyDepth(NCreature ownerNode, List<NCreature> friendNodes)
    {
        var valid = new List<NCreature>();
        foreach (NCreature friend in friendNodes)
        {
            if (GodotObject.IsInstanceValid(friend) && friend.GetParent() is { } p && GodotObject.IsInstanceValid(p))
                valid.Add(friend);
        }

        if (valid.Count == 0)
            return;

        // 按伙伴自己的父容器分组（玩家节点可能不在同一容器里）
        foreach (IGrouping<Node, NCreature> group in valid.GroupBy(n => n.GetParent()))
        {
            Node parent = group.Key;
            List<NCreature> nodes = group.ToList();

            // 主人和伙伴同容器时把主人也纳入排序：主人上方的伙伴在主人后面
            if (ownerNode.GetParent() == parent && GodotObject.IsInstanceValid(ownerNode))
                nodes.Add(ownerNode);

            List<NCreature> ordered = nodes.OrderBy(n => n.Position.Y).ToList();

            // 从这组节点当前占用的最小兄弟索引开始，按 Y 升序连续重排
            int cursor = nodes.Min(n => n.GetIndex(false));
            foreach (NCreature node in ordered)
            {
                if (node.GetIndex(false) != cursor)
                    parent.MoveChild(node, cursor);

                cursor++;
            }
        }
    }
}
