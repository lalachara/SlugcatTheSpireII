using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.relics;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 「只允许蛞蝓猫角色获得」的遗物名单。
/// <para>
/// 这些遗物目前挂在通用遗物池 <c>SharedRelicPool</c> 上（轮回之花在僧侣池 / 典藏书签在通用池），
/// 通用池会被原版角色一起抽取，所以需要在抽取处对非蛞蝓猫角色做剔除。
/// 以后还有别的「仅蛞蝓猫」遗物，往 <see cref="Types"/> 里加一行即可。
/// </para>
/// </summary>
internal static class SlugcatOnlyRelics
{
	internal static readonly HashSet<Type> Types = new()
	{
		typeof(Catrock),   // 典藏书签
		typeof(Flower),    // 轮回之花
		typeof(Redcat_Yoyo), // 悠悠球（通用池里的非先古遗物，原版角色也抽得到）
	};

	internal static bool IsRestricted(RelicModel relic) =>
		relic != null && Types.Contains(relic.GetType());

	/// <summary>单机 / 无玩家信息时：这一局里是否存在蛞蝓猫角色。</summary>
	internal static bool RunHasSlugcat(IRunState runState) =>
		runState?.Players != null && runState.Players.Any(p => p.Character is Slugcat);
}

/// <summary>
/// 战斗奖励 / 商店等「按玩家抽遗物」的路径：非蛞蝓猫角色抽到受限遗物时重新抽。
/// </summary>
public sealed class SlugcatOnlyRelicFilterPatch : IPatchMethod
{
	public static string PatchId => "rainworld_slugcat_only_relic_filter";

	public static string Description => "仅蛞蝓猫能获得的遗物（典藏书签、轮回之花、悠悠球）不对原版角色生成";

	public static bool IsCritical => false;

	[ThreadStatic]
	private static bool _isReplacing;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromFront),
			new[] { typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>) }),
		new(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromBack),
			new[] { typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>) }),
	];

	public static void Postfix(ref RelicModel __result, Player player, RelicRarity rarity, Func<RelicModel, bool> filter)
	{
		if (_isReplacing)
			return;

		if (__result == null)
			return;

		if (player?.Character is Slugcat)
			return;

		if (!SlugcatOnlyRelics.IsRestricted(__result))
			return;

		Entry.Logger.Info($"[遗物过滤] {__result.GetType().Name} 仅蛞蝓猫可获得，为 {player?.Character?.GetType().Name} 重新抽取");

		_isReplacing = true;
		try
		{
			const int maxRetries = 20;
			for (var i = 0; i < maxRetries; i++)
			{
				var newRelic = RelicFactory.PullNextRelicFromFront(player, rarity, filter);
				if (!SlugcatOnlyRelics.IsRestricted(newRelic))
				{
					__result = newRelic;
					return;
				}
			}

			Entry.Logger.Warn($"[遗物过滤] 重试{maxRetries}次后仍未抽到可用的遗物，保留原结果");
		}
		finally
		{
			_isReplacing = false;
		}
	}
}

/// <summary>
/// 宝箱（宝藏房间）的全局遗物抓取袋：整局没有任何蛞蝓猫角色时，把受限遗物从袋里剔掉。
/// 多人里只要场上有蛞蝓猫就保持原样（抓取袋此时无法确定归属，和现有的原版遗物过滤逻辑一致）。
/// </summary>
public sealed class SlugcatOnlyRelicGrabBagFilterPatch : IPatchMethod
{
	public static string PatchId => "rainworld_slugcat_only_relic_grab_bag_filter";

	public static string Description => "宝箱遗物：没有任何蛞蝓猫角色时剔除仅蛞蝓猫可获得的遗物";

	public static bool IsCritical => false;

	[ThreadStatic]
	private static bool _isReplacing;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromFront),
			new[] { typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState) }),
		new(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromBack),
			new[] { typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState) }),
	];

	public static void Postfix(ref RelicModel __result, RelicGrabBag __instance, RelicRarity rarity,
		Func<RelicModel, bool> filter, IRunState runState)
	{
		if (_isReplacing)
			return;

		if (__result == null)
			return;

		// 有蛞蝓猫在场就不动（抓取袋是全局的，无法按玩家区分）。
		if (SlugcatOnlyRelics.RunHasSlugcat(runState))
			return;

		if (!SlugcatOnlyRelics.IsRestricted(__result))
			return;

		Entry.Logger.Info($"[遗物过滤GrabBag] {__result.GetType().Name} 仅蛞蝓猫可获得，本局没有蛞蝓猫，重新抽取");

		_isReplacing = true;
		try
		{
			const int maxRetries = 20;
			for (var i = 0; i < maxRetries; i++)
			{
				var newRelic = __instance.PullFromFront(rarity, filter, runState);
				if (newRelic != null && !SlugcatOnlyRelics.IsRestricted(newRelic))
				{
					__result = newRelic;
					return;
				}
			}

			Entry.Logger.Warn($"[遗物过滤GrabBag] 重试{maxRetries}次后仍未抽到可用的遗物，保留原结果");
		}
		finally
		{
			_isReplacing = false;
		}
	}
}
