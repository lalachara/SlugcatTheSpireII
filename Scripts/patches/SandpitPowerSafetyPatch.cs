using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 无厌沙虫（The Insatiable）的吞噬能力 <see cref="SandpitPower" /> 的防御性补丁。
/// <para>原版 <c>AfterRemoved</c> 里假定「沙虫本体 + 被吞的玩家 + 该玩家<b>所有宠物</b>」都有场景节点，
/// 并且直接用 <c>NCombatRoom.Instance.GetCreatureNode(...).GlobalPosition</c> / <c>.Visuals</c>：</para>
/// <code>
/// NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Owner);
/// float num = ((Control)creatureNode).GlobalPosition.X - 450f;        // 节点为 null → 空引用
/// foreach (Creature c in AllAffectedCreatures) { var n = GetCreatureNode(c); ... }   // 同样不判空
/// </code>
/// <para>只要其中一个单位此刻没有节点（玩家刚被判定死亡/正在被移出场景、宠物已经退场、
/// 或者本 mod 的伙伴在吞噬结算时被移除），就会在这里抛 <see cref="System.NullReferenceException" />，
/// 把整个回合循环打断（表现就是「队友被沙虫吞了之后游戏卡死」）。</para>
/// <para>这里在原方法之前做一次「节点是否齐全」的检查：缺任何一个就直接跳过这段演出，
/// 既不崩也不影响其它逻辑（正常的吞噬演出仍然照原版跑）。</para>
/// </summary>
public sealed class SandpitPowerSafetyPatch : IPatchMethod
{
	public static string PatchId => "rainworld_sandpit_power_safety";
	public static string Description => "无厌沙虫吞噬能力的场景节点判空，避免回合循环空引用崩溃";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(SandpitPower), nameof(SandpitPower.AfterRemoved), new[] { typeof(Creature) })
	];

	public static bool Prefix(SandpitPower __instance, Creature oldOwner, ref Task __result)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		// 原版在开头就会对这两种情况直接 return，交给它自己处理
		if (oldOwner.IsDead || __instance.Target.IsDead)
			return true;

		// 沙虫本体没有节点（例如已经死亡/退场）：原版下一行就会空引用
		if (room.GetCreatureNode(__instance.Owner) == null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		// 被吞的玩家
		if (__instance.Target.Player is not { } player || room.GetCreatureNode(player.Creature) == null)
		{
			__result = Task.CompletedTask;
			return false;
		}

		// 被吞玩家的所有宠物（含本 mod 的伙伴、原版 Osty）
		foreach (Creature pet in __instance.Target.Pets)
		{
			if (room.GetCreatureNode(pet) == null)
			{
				__result = Task.CompletedTask;
				return false;
			}
		}

		return true;
	}
}
