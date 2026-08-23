using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches.Filter;

/// <summary>
/// 棱镜宝石走 ModifyCardRewardCreationOptions 把玩家其他角色的卡池并入奖励池。
/// 非蛞蝓猫角色打开配置后，剔除并入的蛞蝓猫角色主池。
/// </summary>
public sealed class PrismaticGemCardPoolFilterPatch : IPatchMethod
{
	public static string PatchId => "rainworld_prismatic_gem_pool_filter";
	public static string Description => "棱镜宝石检索其它角色卡牌时排除蛞蝓猫卡";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(PrismaticGem), nameof(PrismaticGem.ModifyCardRewardCreationOptions),
			new[] { typeof(Player), typeof(CardCreationOptions) }),
	];

	public static void Postfix(Player player, ref CardCreationOptions __result)
	{
		if (!SlugcatCardFilter.IsConfigEnabled(player))
			return;
		if (player.Character is Slugcat)
			return;

		__result = __result.WithCardPools(__result.CardPools.Where(p => !SlugcatCardFilter.IsSlugcatPool(p)));
	}
}
