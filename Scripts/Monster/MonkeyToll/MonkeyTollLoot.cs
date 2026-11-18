using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>
/// 「拾荒者收费站」战斗的战利品收集器（静态，跨战斗与事件实例存活）：
/// 拾荒者偷到的东西在它死亡时折算成奖励暂存到这里，战斗全歼回到事件后，
/// 由 <see cref="Rainworld.Scripts.Event.ScavengerEvent" /> 在 Resume 里统一通过奖励屏发还。
/// 多人模式下按玩家分开记账，被偷的东西只会还给它本来的主人。
/// </summary>
public static class MonkeyTollLoot
{
	private static readonly Dictionary<ulong, List<Reward>> Loot = [];

	/// <summary>本场战斗是否有任意一只拾荒者逃走（有则事件直接结束，不再回战利品结算页）。</summary>
	public static bool AnyEscaped { get; private set; }

	/// <summary>遭遇战生成怪物（开战前）时清空上一场的残留状态。</summary>
	public static void Reset()
	{
		Loot.Clear();
		AnyEscaped = false;
	}

	/// <summary>有拾荒者成功逃走（对应塔1「有猴子逃走则不回事件」）。</summary>
	public static void MarkEscaped() => AnyEscaped = true;

	/// <summary>把一份奖励挂到指定玩家名下。</summary>
	public static void Add(Player player, Reward reward)
	{
		if (!Loot.TryGetValue(player.NetId, out List<Reward>? list))
		{
			list = [];
			Loot[player.NetId] = list;
		}
		list.Add(reward);
	}

	/// <summary>取走该玩家名下的全部奖励（没有则返回空列表）。</summary>
	public static List<Reward> TakeFor(Player player) =>
		Loot.Remove(player.NetId, out List<Reward>? list) ? list : [];
}
