using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches.Filter;

/// <summary>
/// 飞溅（Splash）走 CardFactory.GetDistinctForCombat 取「其他角色的攻击牌」。
/// 非蛞蝓猫角色打开配置后，将传入候选卡里属于蛞蝓猫角色主池的卡剔除。
/// </summary>
public sealed class SplashCardPoolFilterPatch : IPatchMethod
{
	public static string PatchId => "rainworld_splash_pool_filter";
	public static string Description => "飞溅检索其它角色卡牌时排除蛞蝓猫卡";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardFactory), nameof(CardFactory.GetDistinctForCombat),
			new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(int), typeof(MegaCrit.Sts2.Core.Random.Rng) }),
	];

	public static void Prefix(Player player, ref IEnumerable<CardModel> cards)
	{
		if (!SlugcatCardFilter.IsConfigEnabled(player))
			return;
		if (player.Character is Slugcat)
			return;

		cards = cards.Where(c => !SlugcatCardFilter.IsSlugcatCard(c));
	}
}
